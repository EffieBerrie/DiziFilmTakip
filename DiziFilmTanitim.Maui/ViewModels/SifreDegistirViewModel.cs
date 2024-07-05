using System.Windows.Input;
using DiziFilmTanitim.MAUI.Services;
using DiziFilmTanitim.MAUI.Models; // CommonApiResponseModel için using direktifi
// using DiziFilmTanitim.MAUI.Models; // Eğer SifreDegistirRequest gibi bir model gerekirse eklenecek

namespace DiziFilmTanitim.MAUI.ViewModels
{
    public class SifreDegistirViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private readonly IUserService _userService;
        private readonly ILoggingService _logger;

        private string _eskiSifre = string.Empty;
        private string _yeniSifre = string.Empty;
        private string _yeniSifreTekrar = string.Empty;
        private string _hataMesaji = string.Empty;
        private string _basariMesaji = string.Empty;
        private bool _hataVarMi;
        private bool _basariVarMi;

        public SifreDegistirViewModel(IApiService apiService, IUserService userService, ILoggingService logger)
        {
            _apiService = apiService;
            _userService = userService;
            _logger = logger;
            Title = "Şifre Değiştir";

            SifreDegistirCommand = new Command(async () => await SifreDegistirAsync(), CanSifreDegistir);
            GeriGitCommand = new Command(async () => await GeriGitAsync(), () => IsNotBusy);
            PropertyChanged += (_, __) =>
            {
                ((Command)SifreDegistirCommand).ChangeCanExecute();
                ((Command)GeriGitCommand).ChangeCanExecute();
            };
        }

        public string EskiSifre
        {
            get => _eskiSifre;
            set => SetProperty(ref _eskiSifre, value, () => ((Command)SifreDegistirCommand).ChangeCanExecute());
        }

        public string YeniSifre
        {
            get => _yeniSifre;
            set => SetProperty(ref _yeniSifre, value, () => ((Command)SifreDegistirCommand).ChangeCanExecute());
        }

        public string YeniSifreTekrar
        {
            get => _yeniSifreTekrar;
            set => SetProperty(ref _yeniSifreTekrar, value, () => ((Command)SifreDegistirCommand).ChangeCanExecute());
        }

        public string HataMesaji { get => _hataMesaji; private set => SetProperty(ref _hataMesaji, value); }
        public string BasariMesaji { get => _basariMesaji; private set => SetProperty(ref _basariMesaji, value); }
        public bool HataVarMi { get => _hataVarMi; private set => SetProperty(ref _hataVarMi, value); }
        public bool BasariVarMi { get => _basariVarMi; private set => SetProperty(ref _basariVarMi, value); }

        public ICommand SifreDegistirCommand { get; }
        public ICommand GeriGitCommand { get; }

        private bool CanSifreDegistir()
        {
            return !IsBusy &&
                   !string.IsNullOrWhiteSpace(EskiSifre) &&
                   !string.IsNullOrWhiteSpace(YeniSifre) &&
                   !string.IsNullOrWhiteSpace(YeniSifreTekrar) &&
                   YeniSifre.Length >= 6;
        }

        private async Task SifreDegistirAsync()
        {
            if (!ValidateForm()) return;

            if (!_userService.IsLoggedIn || !_userService.CurrentUserId.HasValue)
            {
                HataGoster("Bu işlem için giriş yapmış olmalısınız.");
                return;
            }

            try
            {
                IsBusy = true;
                MesajlariTemizle();

                var requestData = new { EskiSifre, YeniSifre }; // API'nin beklediği SifreDegistirModel'e uygun anonim tip

                // API endpoint'i /api/kullanicilar/{id}/sifre-degistir şeklinde.
                var response = await _apiService.PutAsync<CommonApiResponseModel>($"api/kullanicilar/{_userService.CurrentUserId.Value}/sifre-degistir", requestData);

                // Response null ise (ağ hatası vb.)
                if (response == null)
                {
                    _logger.LogWarning("Şifre değiştirme - API'den null response alındı");
                    HataGoster("Sunucuya bağlanılamadı. Lütfen internet bağlantınızı kontrol edin.");
                    return;
                }

                // API'den başarılı response geldi
                if (response.Success)
                {
                    _logger.LogInfo($"Kullanıcı (ID: {_userService.CurrentUserId.Value}) şifresini başarıyla değiştirdi.");
                    BasariGoster("Şifreniz başarıyla değiştirildi.");
                    // Formu temizle
                    EskiSifre = string.Empty;
                    YeniSifre = string.Empty;
                    YeniSifreTekrar = string.Empty;
                }
                else
                {
                    // API'den hata response'u geldi
                    string hataMesaji = "Şifre değiştirilemedi. Lütfen bilgilerinizi kontrol edin.";

                    // API'den gelen özel hata mesajlarını kontrol et
                    if (!string.IsNullOrEmpty(response.Message))
                    {
                        if (response.Message.Contains("Eski şifre yanlış") || response.Message.Contains("eski şifre") ||
                            response.Message.Contains("current password") || response.Message.Contains("yanlış") ||
                            response.Message.Contains("wrong") || response.Message.Contains("incorrect"))
                            hataMesaji = "Eski şifreniz yanlış. Lütfen doğru şifreyi girin.";
                        else if (response.Message.Contains("zayıf") || response.Message.Contains("weak"))
                            hataMesaji = "Yeni şifreniz çok zayıf. Daha güçlü bir şifre seçin.";
                        else if (response.Message.Contains("bulunamadı") || response.Message.Contains("not found"))
                            hataMesaji = "Kullanıcı bulunamadı. Lütfen yeniden giriş yapın.";
                        else
                            hataMesaji = response.Message;
                    }

                    _logger.LogWarning($"Şifre değiştirme başarısız. Kullanıcı ID: {_userService.CurrentUserId.Value}. API Yanıtı: {response.Message}");
                    HataGoster(hataMesaji);
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError($"HTTP Hatası: {httpEx.Message}", httpEx);
                HataGoster("Sunucuya bağlanılamadı. Lütfen internet bağlantınızı kontrol edin.");
            }
            catch (TaskCanceledException tcEx)
            {
                _logger.LogError($"Timeout Hatası: {tcEx.Message}", tcEx);
                HataGoster("İşlem zaman aşımına uğradı. Lütfen tekrar deneyin.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Şifre değiştirme hatası: {ex.Message}", ex);
                HataGoster("Şifre değiştirme sırasında beklenmeyen bir hata oluştu. Lütfen tekrar deneyin.");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool ValidateForm()
        {
            MesajlariTemizle();
            if (string.IsNullOrWhiteSpace(EskiSifre))
            {
                HataGoster("Eski şifre boş olamaz.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(YeniSifre))
            {
                HataGoster("Yeni şifre boş olamaz.");
                return false;
            }
            if (YeniSifre.Length < 6)
            {
                HataGoster("Yeni şifre en az 6 karakter olmalıdır.");
                return false;
            }
            if (YeniSifre != YeniSifreTekrar)
            {
                HataGoster("Yeni şifreler eşleşmiyor.");
                return false;
            }
            if (EskiSifre == YeniSifre)
            {
                HataGoster("Yeni şifre eski şifreyle aynı olamaz.");
                return false;
            }
            return true;
        }

        private void HataGoster(string mesaj)
        {
            HataMesaji = mesaj;
            HataVarMi = true;
            BasariVarMi = false;
            _logger.LogInfo($"Hata mesajı gösteriliyor: {mesaj}");
        }

        private void BasariGoster(string mesaj)
        {
            BasariMesaji = mesaj;
            BasariVarMi = true;
            HataVarMi = false;
        }

        private void MesajlariTemizle()
        {
            HataMesaji = string.Empty;
            HataVarMi = false;
            BasariMesaji = string.Empty;
            BasariVarMi = false;
        }

        private async Task GeriGitAsync()
        {
            if (IsBusy) return;
            await Shell.Current.GoToAsync("..");
        }
    }
}