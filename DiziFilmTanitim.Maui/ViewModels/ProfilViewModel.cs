using System.Windows.Input;
using DiziFilmTanitim.MAUI.Services;

namespace DiziFilmTanitim.MAUI.ViewModels
{
    public class ProfilViewModel : BaseViewModel
    {
        private readonly IUserService _userService;
        private readonly IApiService _apiService; // Token temizleme için
        private readonly ILoggingService _logger;

        private string? _kullaniciAdi;
        private string? _email;
        private bool _isLoggedIn;

        public ProfilViewModel(IUserService userService, IApiService apiService, ILoggingService logger)
        {
            _userService = userService;
            _apiService = apiService;
            _logger = logger;
            Title = "Profilim";

            CikisYapCommand = new Command(async () => await CikisYapAsync(), () => IsNotBusy && IsLoggedIn);
            SifreDegistirSayfasinaGitCommand = new Command(async () => await SifreDegistirSayfasinaGitAsync(), () => IsNotBusy && IsLoggedIn);
            GirisYapCommand = new Command(async () => await GirisSayfasinaGitAsync(), () => IsNotBusy && !IsLoggedIn);

            KullaniciBilgileriniYukle();
        }

        public string? KullaniciAdi
        {
            get => _kullaniciAdi;
            set => SetProperty(ref _kullaniciAdi, value);
        }

        public string? Email // Kullanıcı e-postası API veya UserService üzerinden geliyorsa burası doldurulacak
        {
            get => _email;
            // API'den kullanıcı detayları çekilirse veya UserService'de email tutulursa set edilecek.
            // Şimdilik sadece IUserService'deki CurrentUserName ve CurrentUserId var.
            // Email göstermek için API'den kullanıcı detaylarını çekmemiz gerekebilir.
            // Ya da kayıt/giriş sırasında email de saklanabilir UserService içinde (SecureStorage).
            // Basitlik adına şimdilik sadece KullaniciAdi'nı gösterelim.
            // Email için TODO: API'den kullanıcı detaylarını getirme veya UserService'de saklama.
            set => SetProperty(ref _email, value);
        }

        public bool IsLoggedIn // Yeni özellik
        {
            get => _isLoggedIn;
            private set => SetProperty(ref _isLoggedIn, value);
        }

        public ICommand CikisYapCommand { get; }
        public ICommand SifreDegistirSayfasinaGitCommand { get; }
        public ICommand GirisYapCommand { get; }

        public void KullaniciBilgileriniYukle() // Sayfa göründüğünde de çağrılabilir
        {
            IsLoggedIn = _userService.IsLoggedIn; // IsLoggedIn durumu güncelleniyor
            if (IsLoggedIn)
            {
                KullaniciAdi = _userService.CurrentUserName;
                // Email = _userService.CurrentUserEmail; // Eğer UserService'de email tutuluyorsa
                _logger.LogInfo($"Profil sayfası için kullanıcı bilgileri yüklendi: {KullaniciAdi}");
            }
            else
            {
                KullaniciAdi = "Misafir"; // Bu kalabilir veya XAML'de IsLoggedIn'e göre farklı mesaj gösterilebilir
                Email = string.Empty;
                _logger.LogWarning("Profil sayfası: Kullanıcı giriş yapmamış.");
            }
            // Komutların CanExecute durumlarını güncelle
            ((Command)CikisYapCommand).ChangeCanExecute();
            ((Command)SifreDegistirSayfasinaGitCommand).ChangeCanExecute();
            ((Command)GirisYapCommand).ChangeCanExecute();
        }

        private async Task CikisYapAsync()
        {
            if (IsBusy) return;

            bool eminMisin = await Application.Current.MainPage.DisplayAlert(
                "Çıkış Yap",
                "Çıkış yapmak istediğinizden emin misiniz?",
                "Evet, Çıkış Yap", "Hayır");

            if (!eminMisin) return;

            try
            {
                IsBusy = true;
                _logger.LogInfo("Çıkış yapılıyor...");

                _userService.ClearCurrentUser(); // SecureStorage'dan siler ve bellekteki bilgileri temizler
                _apiService.ClearAuthToken();    // HttpClient'daki token'ı temizler

                _logger.LogInfo("Başarıyla çıkış yapıldı. Giriş sayfasına yönlendiriliyor.");
                await Shell.Current.GoToAsync("//GirisSayfasi");
            }
            catch (Exception ex)
            {
                _logger.LogError("Çıkış yapma hatası", ex);
                await Application.Current.MainPage.DisplayAlert("Hata", "Çıkış yapılırken bir sorun oluştu.", "Tamam");
            }
            finally
            {
                IsBusy = false;
                ((Command)CikisYapCommand).ChangeCanExecute();
                ((Command)SifreDegistirSayfasinaGitCommand).ChangeCanExecute();
                ((Command)GirisYapCommand).ChangeCanExecute();
            }
        }

        private async Task SifreDegistirSayfasinaGitAsync()
        {
            // SifreDegistirSayfasi artık oluşturuldu.
            _logger.LogDebug("Şifre Değiştir sayfasına gidiliyor...");
            await Shell.Current.GoToAsync("SifreDegistirSayfasi");
        }

        private async Task GirisSayfasinaGitAsync()
        {
            if (IsBusy) return;
            await Shell.Current.GoToAsync("//GirisSayfasi");
        }
    }
}