using System.Collections.ObjectModel;
using System.Windows.Input;
using DiziFilmTanitim.MAUI.Models; // KullaniciListesiResponseModel için
using DiziFilmTanitim.MAUI.Services;

namespace DiziFilmTanitim.MAUI.ViewModels
{
    public class ListelerViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private readonly IUserService _userService;
        private readonly ILoggingService _logger;

        private ObservableCollection<KullaniciListesiResponseModel> _kullaniciListeleri = new();
        private bool _isLoggedIn;

        public ListelerViewModel(IApiService apiService, IUserService userService, ILoggingService logger)
        {
            _apiService = apiService;
            _userService = userService;
            _logger = logger;
            Title = "Listelerim";

            GirisYapCommand = new Command(async () => await GirisSayfasinaGitAsync(), () => !IsBusy && !IsLoggedIn);
            YeniListeOlusturCommand = new Command(async () => await YeniListeOlusturAsync(), () => !IsBusy && IsLoggedIn);
            ListeSecCommand = new Command<KullaniciListesiResponseModel>(async (liste) => await ListeDetayinaGitAsync(liste));

            // Sayfa ilk yüklendiğinde veya her göründüğünde durum kontrolü ve veri yükleme OnAppearing'de tetiklenecek.
        }

        public ObservableCollection<KullaniciListesiResponseModel> KullaniciListeleri
        {
            get => _kullaniciListeleri;
            set => SetProperty(ref _kullaniciListeleri, value);
        }

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            private set
            {
                if (SetProperty(ref _isLoggedIn, value))
                {
                    ((Command)GirisYapCommand).ChangeCanExecute();
                    ((Command)YeniListeOlusturCommand).ChangeCanExecute();
                    OnPropertyChanged(nameof(VeriYok)); // IsLoggedIn değişince VeriYok da değişebilir
                }
            }
        }

        public bool VeriYok => IsLoggedIn && !KullaniciListeleri.Any();

        public ICommand GirisYapCommand { get; }
        public ICommand YeniListeOlusturCommand { get; }
        public ICommand ListeSecCommand { get; } // Listeden bir öğe seçildiğinde çalışacak

        public async Task SayfaGorunurOldugundaAsync()
        {
            IsLoggedIn = _userService.IsLoggedIn;
            if (IsLoggedIn)
            {
                await KullaniciListeleriniYukleAsync();
            }
            else
            {
                KullaniciListeleri.Clear(); // Giriş yapılmamışsa listeyi temizle
                OnPropertyChanged(nameof(VeriYok));
            }
        }

        private async Task KullaniciListeleriniYukleAsync()
        {
            if (!IsLoggedIn || _userService.CurrentUserId == null)
            {
                KullaniciListeleri.Clear();
                OnPropertyChanged(nameof(VeriYok));
                return;
            }

            try
            {
                IsBusy = true;
                var listeler = await _apiService.GetAsync<KullaniciListesiResponseModel[]>($"api/kullanici-listeleri/kullanici/{_userService.CurrentUserId.Value}");

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    KullaniciListeleri.Clear();
                    if (listeler != null)
                    {
                        foreach (var liste in listeler)
                        {
                            KullaniciListeleri.Add(liste);
                        }
                    }
                    OnPropertyChanged(nameof(VeriYok));
                });
                _logger.LogInfo($"{KullaniciListeleri.Count} kullanıcı listesi yüklendi.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Kullanıcı listeleri yüklenirken hata oluştu.", ex);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    KullaniciListeleri.Clear();
                    OnPropertyChanged(nameof(VeriYok));
                });
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task GirisSayfasinaGitAsync()
        {
            await Shell.Current.GoToAsync("//GirisSayfasi");
        }

        private async Task YeniListeOlusturAsync()
        {
            if (!IsLoggedIn || _userService.CurrentUserId == null)
            {
                _logger.LogWarning("Yeni liste oluşturma denemesi, ancak kullanıcı giriş yapmamış veya ID'si yok.");
                await Application.Current.MainPage.DisplayAlert("Hata", "Liste oluşturmak için giriş yapmalısınız.", "Tamam");
                return;
            }

            try
            {
                string listeAdi = await Application.Current.MainPage.DisplayPromptAsync(
                    "Yeni Liste",
                    "Listenizin adı ne olsun?",
                    "Oluştur", "İptal",
                    "Örn: İzlenecek Filmler",
                    maxLength: 100,
                    keyboard: Keyboard.Text);

                if (string.IsNullOrWhiteSpace(listeAdi))
                {
                    _logger.LogDebug("Liste oluşturma iptal edildi veya boş isim girildi.");
                    return; // Kullanıcı iptal etti veya boş isim girdi
                }

                string? aciklama = await Application.Current.MainPage.DisplayPromptAsync(
                    "Liste Açıklaması (İsteğe Bağlı)",
                    "Listeniz için bir açıklama ekleyin (isteğe bağlı):",
                    "Kaydet", "Atla",
                    "Örn: Bu yıl bitirmeyi planladığım filmler",
                    maxLength: 255,
                    keyboard: Keyboard.Text);

                IsBusy = true;

                var listeRequest = new KullaniciListesiEkleRequest(listeAdi.Trim(), string.IsNullOrWhiteSpace(aciklama) ? null : aciklama.Trim());
                var yeniListe = await _apiService.CreateKullaniciListesiAsync<KullaniciListesiResponseModel>(_userService.CurrentUserId.Value, listeRequest);

                if (yeniListe != null && yeniListe.Id > 0)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        KullaniciListeleri.Add(yeniListe);
                        OnPropertyChanged(nameof(VeriYok)); // Liste eklendiği için VeriYok durumu güncellenmeli
                    });
                    _logger.LogInfo($"Yeni liste başarıyla oluşturuldu: {yeniListe.ListeAdi} (ID: {yeniListe.Id})");
                    await Application.Current.MainPage.DisplayAlert("Başarılı", $"'{yeniListe.ListeAdi}' adlı listeniz oluşturuldu!", "Harika!");
                }
                else
                {
                    _logger.LogError("API'den yeni liste oluşturma yanıtı başarısız veya geçersiz.");
                    await Application.Current.MainPage.DisplayAlert("Hata", "Liste oluşturulurken bir sorun oluştu. Lütfen tekrar deneyin.", "Tamam");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("YeniListeOlusturAsync sırasında bir hata oluştu.", ex);
                await Application.Current.MainPage.DisplayAlert("Hata", "Beklenmedik bir sorun oluştu. Detaylar için logları kontrol edin.", "Tamam");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ListeDetayinaGitAsync(KullaniciListesiResponseModel? liste)
        {
            if (liste == null) return;
            _logger.LogDebug($"Liste detayına gidiliyor: {liste.ListeAdi} (ID: {liste.Id})");
            // KullaniciListeDetaySayfasi artık oluşturuldu.
            await Shell.Current.GoToAsync($"KullaniciListeDetay?listeId={liste.Id}");
        }
    }
}