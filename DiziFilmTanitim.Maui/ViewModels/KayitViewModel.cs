using System.Windows.Input;
using DiziFilmTanitim.MAUI.Models;
using DiziFilmTanitim.MAUI.Services;
using System.Text.RegularExpressions;

namespace DiziFilmTanitim.MAUI.ViewModels
{
    public class KayitViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private readonly ILoggingService _logger;
        private string _kullaniciAdi = string.Empty;
        private string _eposta = string.Empty;
        private string _sifre = string.Empty;
        private string _sifreTekrar = string.Empty;
        private string _hataMesaji = string.Empty;
        private string _basariMesaji = string.Empty;
        private bool _hataVarMi;
        private bool _basariVarMi;

        public KayitViewModel(IApiService apiService, ILoggingService logger)
        {
            _apiService = apiService;
            _logger = logger;
            Title = "Kayıt Ol";

            // Command'ları başlat
            KayitCommand = new Command(async () => await KayitYapAsync(), () => !IsBusy);
            GirisSayfasinaGitCommand = new Command(async () => await GirisSayfasinaGitAsync());
            MisafirGirisCommand = new Command(async () => await MisafirGirisAsync());
        }

        // Properties
        public string KullaniciAdi
        {
            get => _kullaniciAdi;
            set => SetProperty(ref _kullaniciAdi, value);
        }

        public string Eposta
        {
            get => _eposta;
            set => SetProperty(ref _eposta, value);
        }

        public string Sifre
        {
            get => _sifre;
            set => SetProperty(ref _sifre, value);
        }

        public string SifreTekrar
        {
            get => _sifreTekrar;
            set => SetProperty(ref _sifreTekrar, value);
        }

        public string HataMesaji
        {
            get => _hataMesaji;
            set => SetProperty(ref _hataMesaji, value);
        }

        public string BasariMesaji
        {
            get => _basariMesaji;
            set => SetProperty(ref _basariMesaji, value);
        }

        public bool HataVarMi
        {
            get => _hataVarMi;
            set => SetProperty(ref _hataVarMi, value);
        }

        public bool BasariVarMi
        {
            get => _basariVarMi;
            set => SetProperty(ref _basariVarMi, value);
        }

        public bool IsNotBusy => !IsBusy;

        // Commands
        public ICommand KayitCommand { get; }
        public ICommand GirisSayfasinaGitCommand { get; }
        public ICommand MisafirGirisCommand { get; }

        // Methods
        private async Task KayitYapAsync()
        {
            if (IsBusy) return;

            // Form validation (İster 21)
            if (!ValidateForm())
                return;

            try
            {
                IsBusy = true;
                MesajlariTemizle();

                var kayitRequest = new KayitRequest
                {
                    KullaniciAdi = KullaniciAdi.Trim(),
                    Eposta = Eposta.Trim(),
                    Sifre = Sifre
                };

                // API çağrısı
                var response = await _apiService.PostAsync<KayitResponse>("api/kullanicilar/kayit", kayitRequest);

                if (response != null && response.Id > 0)
                {
                    // Başarılı kayıt
                    _logger.LogDebug($"Başarılı kayıt - Kullanıcı: {response.KullaniciAdi}");
                    BasariGoster("Kayıt başarılı! Giriş sayfasına yönlendiriliyorsunuz...");

                    // 2 saniye bekle ve giriş sayfasına git
                    await Task.Delay(2000);
                    await Shell.Current.GoToAsync("//GirisSayfasi");
                }
                else
                {
                    _logger.LogDebug("Kayıt başarısız - Response null veya geçersiz ID");
                    HataGoster(response?.Message ?? "Kayıt işlemi başarısız. Lütfen tekrar deneyin.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Kayıt hatası: {ex.Message}", ex);
                HataGoster($"Kayıt sırasında hata oluştu: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
                ((Command)KayitCommand).ChangeCanExecute(); // Command'ı güncelle
            }
        }

        private async Task GirisSayfasinaGitAsync()
        {
            await Shell.Current.GoToAsync("//GirisSayfasi");
        }

        private async Task MisafirGirisAsync()
        {
            await Shell.Current.GoToAsync("//MainTabs/AnaSayfa");
        }

        private bool ValidateForm()
        {
            // Kullanıcı adı kontrolü
            if (string.IsNullOrWhiteSpace(KullaniciAdi))
            {
                HataGoster("Kullanıcı adı boş olamaz.");
                return false;
            }

            if (KullaniciAdi.Length < 3)
            {
                HataGoster("Kullanıcı adı en az 3 karakter olmalıdır.");
                return false;
            }

            // E-posta kontrolü
            if (string.IsNullOrWhiteSpace(Eposta))
            {
                HataGoster("E-posta adresi boş olamaz.");
                return false;
            }

            if (!IsValidEmail(Eposta))
            {
                HataGoster("Geçerli bir e-posta adresi girin.");
                return false;
            }

            // Şifre kontrolü
            if (string.IsNullOrWhiteSpace(Sifre))
            {
                HataGoster("Şifre boş olamaz.");
                return false;
            }

            if (Sifre.Length < 6)
            {
                HataGoster("Şifre en az 6 karakter olmalıdır.");
                return false;
            }

            // Şifre tekrar kontrolü
            if (Sifre != SifreTekrar)
            {
                HataGoster("Şifreler eşleşmiyor.");
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                return Regex.IsMatch(email, emailPattern);
            }
            catch
            {
                return false;
            }
        }

        private void HataGoster(string mesaj)
        {
            HataMesaji = mesaj;
            HataVarMi = true;
            BasariVarMi = false;
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
    }
}