using System.Windows.Input;
using DiziFilmTanitim.MAUI.Models;
using DiziFilmTanitim.MAUI.Services;

namespace DiziFilmTanitim.MAUI.ViewModels
{
    public class GirisViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private readonly IUserService _userService;
        private readonly ILoggingService _logger;
        private string _kullaniciAdi = string.Empty;
        private string _sifre = string.Empty;
        private string _hataMesaji = string.Empty;
        private bool _hataVarMi;

        public GirisViewModel(IApiService apiService, IUserService userService, ILoggingService logger)
        {
            _apiService = apiService;
            _userService = userService;
            _logger = logger;
            Title = "Giriş Yap";

            // Command'ları başlat
            GirisCommand = new Command(async () => await GirisYapAsync(), () => !IsBusy);
            KayitSayfasinaGitCommand = new Command(async () => await KayitSayfasinaGitAsync());
            MisafirGirisCommand = new Command(async () => await MisafirGirisAsync());
        }

        // Properties
        public string KullaniciAdi
        {
            get => _kullaniciAdi;
            set => SetProperty(ref _kullaniciAdi, value);
        }

        public string Sifre
        {
            get => _sifre;
            set => SetProperty(ref _sifre, value);
        }

        public string HataMesaji
        {
            get => _hataMesaji;
            set => SetProperty(ref _hataMesaji, value);
        }

        public bool HataVarMi
        {
            get => _hataVarMi;
            set => SetProperty(ref _hataVarMi, value);
        }

        public bool IsNotBusy => !IsBusy;

        // Commands
        public ICommand GirisCommand { get; }
        public ICommand KayitSayfasinaGitCommand { get; }
        public ICommand MisafirGirisCommand { get; }

        // Methods
        private async Task GirisYapAsync()
        {
            if (IsBusy) return;

            if (!ValidateForm())
                return;

            try
            {
                IsBusy = true;
                HataTemizle();

                // Debug log
                _logger.LogDebug($"Giriş denemesi - Kullanıcı: {KullaniciAdi}");

                var loginRequest = new LoginRequest
                {
                    KullaniciAdi = KullaniciAdi.Trim(),
                    Sifre = Sifre
                };

                // API çağrısı
                var response = await _apiService.PostAsync<LoginResponse>("api/kullanicilar/giris", loginRequest);

                _logger.LogDebug($"API Response - ID: {response?.Id}, Kullanıcı: {response?.KullaniciAdi}");

                if (response != null && response.Id > 0)
                {
                    // Başarılı giriş - UserService'e kullanıcı bilgilerini kaydet
                    await _userService.SetUserSessionAsync(response.Id, response.KullaniciAdi);

                    _logger.LogDebug("Başarılı giriş, kullanıcı bilgileri kaydedildi, navigation yapılıyor...");
                    await Shell.Current.GoToAsync("//MainTabs/AnaSayfa");
                }
                else
                {
                    _logger.LogDebug("Giriş başarısız - Response null veya geçersiz ID");
                    HataGoster(response?.Message ?? "Kullanıcı adı veya şifre hatalı.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Giriş hatası: {ex.Message}", ex);
                HataGoster($"Giriş sırasında hata oluştu: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
                ((Command)GirisCommand).ChangeCanExecute(); // Command'ı güncelle
            }
        }

        private async Task KayitSayfasinaGitAsync()
        {
            await Shell.Current.GoToAsync("KayitSayfasi");
        }

        private async Task MisafirGirisAsync()
        {
            await Shell.Current.GoToAsync("//MainTabs/AnaSayfa");
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(KullaniciAdi))
            {
                HataGoster("Kullanıcı adı boş olamaz.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Sifre))
            {
                HataGoster("Şifre boş olamaz.");
                return false;
            }

            if (KullaniciAdi.Length < 3)
            {
                HataGoster("Kullanıcı adı en az 3 karakter olmalıdır.");
                return false;
            }

            return true;
        }

        private void HataGoster(string mesaj)
        {
            HataMesaji = mesaj;
            HataVarMi = true;
        }

        private void HataTemizle()
        {
            HataMesaji = string.Empty;
            HataVarMi = false;
        }
    }
}