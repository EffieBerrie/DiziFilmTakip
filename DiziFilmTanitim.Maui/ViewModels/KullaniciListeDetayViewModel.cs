using System.Collections.ObjectModel;
using System.Windows.Input;
using DiziFilmTanitim.MAUI.Models;
using DiziFilmTanitim.MAUI.Services;
using DiziFilmTanitim.Core.Entities; // DiziDurumu enum için

namespace DiziFilmTanitim.MAUI.ViewModels
{
    // Kullanıcı listesi detay sayfasında gösterilecek filmler için item view model
    public class ListeDetayFilmItemViewModel : BaseViewModel
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string? AfisUrl { get; set; }
        public string YapimYiliText => YapimYili?.ToString() ?? "-";
        public int? YapimYili { get; set; }

        private ICommand? _filmKaldirCommand;
        public ICommand? FilmKaldirCommand
        {
            get => _filmKaldirCommand;
            set => SetProperty(ref _filmKaldirCommand, value);
        }
    }

    // Kullanıcı listesi detay sayfasında gösterilecek diziler için item view model
    public class ListeDetayDiziItemViewModel : BaseViewModel
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string? AfisUrl { get; set; }
        public string YapimYiliText => YapimYili?.ToString() ?? "-";
        public int? YapimYili { get; set; }
        public string? Durum { get; set; }

        private ICommand? _diziKaldirCommand;
        public ICommand? DiziKaldirCommand
        {
            get => _diziKaldirCommand;
            set => SetProperty(ref _diziKaldirCommand, value);
        }
    }

    public class KullaniciListeDetayViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private readonly IUserService _userService;
        private readonly ILoggingService _logger;

        private int _listeId;
        private string _listeAdi = string.Empty;
        private string? _listeAciklama;
        private ObservableCollection<ListeDetayFilmItemViewModel> _filmler = new();
        private ObservableCollection<ListeDetayDiziItemViewModel> _diziler = new();

        public KullaniciListeDetayViewModel(IApiService apiService, IUserService userService, ILoggingService logger)
        {
            _apiService = apiService;
            _userService = userService;
            _logger = logger;
            Title = "Liste Detayı"; // Dinamik olarak liste adına göre güncellenecek

            // Commands
            GeriGitCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }

        public int ListeId
        {
            get => _listeId;
            set
            {
                if (SetProperty(ref _listeId, value))
                {
                    // ListeId değiştiğinde verileri yükle
                    _ = Task.Run(async () => await VerileriYukleAsync());
                }
            }
        }

        public string ListeAdi
        {
            get => _listeAdi;
            set
            {
                if (SetProperty(ref _listeAdi, value))
                {
                    Title = value; // Sayfa başlığını liste adına göre güncelle
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        public string? ListeAciklama
        {
            get => _listeAciklama;
            set => SetProperty(ref _listeAciklama, value);
        }

        public ObservableCollection<ListeDetayFilmItemViewModel> Filmler
        {
            get => _filmler;
            set => SetProperty(ref _filmler, value);
        }

        public ObservableCollection<ListeDetayDiziItemViewModel> Diziler
        {
            get => _diziler;
            set => SetProperty(ref _diziler, value);
        }

        public bool FilmYok => !Filmler.Any();
        public bool DiziYok => !Diziler.Any();

        // Public refresh metodu - sayfa her göründüğünde çağrılacak
        public async Task RefreshAsync()
        {
            await VerileriYukleAsync();
        }

        private string GetDiziDurumuText(DiziDurumu durum)
        {
            return durum switch
            {
                DiziDurumu.Duyuruldu => "Duyuruldu",
                DiziDurumu.DevamEdiyor => "Devam Ediyor",
                DiziDurumu.Tamamlandi => "Tamamlandı",
                DiziDurumu.IptalEdildi => "İptal Edildi",
                DiziDurumu.AraVerdi => "Ara Verdi",
                _ => "Bilinmiyor"
            };
        }

        private async Task VerileriYukleAsync()
        {
            if (ListeId == 0) return;

            try
            {
                IsBusy = true;
                // Önce liste detaylarını çek
                var listeDetay = await _apiService.GetAsync<KullaniciListesiResponseModel>($"api/kullanici-listeleri/{ListeId}");
                if (listeDetay != null)
                {
                    ListeAdi = listeDetay.ListeAdi;
                    ListeAciklama = listeDetay.Aciklama;
                }
                else
                {
                    _logger.LogWarning($"Liste detayı bulunamadı: ID {ListeId}");
                    // Hata mesajı gösterilebilir veya sayfa kapatılabilir
                    await Shell.Current.GoToAsync("..?listeBulunamadi=true"); // Örnek bir geri dönüş
                    return;
                }

                // Sonra listedeki filmleri ve dizileri paralel olarak çek
                var filmlerTask = _apiService.GetAsync<ListeFilmResponseModel[]>($"api/kullanici-listeleri/{ListeId}/filmler");
                var dizilerTask = _apiService.GetAsync<ListeDiziResponseModel[]>($"api/kullanici-listeleri/{ListeId}/diziler");

                await Task.WhenAll(filmlerTask, dizilerTask);

                var filmlerResponse = await filmlerTask;
                var dizilerResponse = await dizilerTask;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Filmler.Clear();
                    if (filmlerResponse != null)
                    {
                        foreach (var film in filmlerResponse)
                        {
                            Filmler.Add(new ListeDetayFilmItemViewModel
                            {
                                Id = film.Id,
                                Ad = film.Ad,
                                YapimYili = film.YapimYili,
                                AfisUrl = !string.IsNullOrEmpty(film.AfisDosyaAdi) ? $"http://localhost:5097/uploads/afisler/{film.AfisDosyaAdi}" : null,
                                FilmKaldirCommand = new Command(async () => await FilmKaldirAsync(film.Id))
                            });
                        }
                    }
                    OnPropertyChanged(nameof(FilmYok));

                    Diziler.Clear();
                    if (dizilerResponse != null)
                    {
                        foreach (var dizi in dizilerResponse)
                        {
                            Diziler.Add(new ListeDetayDiziItemViewModel
                            {
                                Id = dizi.Id,
                                Ad = dizi.Ad,
                                YapimYili = dizi.YapimYili,
                                AfisUrl = !string.IsNullOrEmpty(dizi.AfisDosyaAdi) ? $"http://localhost:5097/uploads/afisler/{dizi.AfisDosyaAdi}" : null,
                                Durum = GetDiziDurumuText((DiziDurumu)dizi.Durum),
                                DiziKaldirCommand = new Command(async () => await DiziKaldirAsync(dizi.Id))
                            });
                        }
                    }
                    OnPropertyChanged(nameof(DiziYok));
                });

                _logger.LogInfo($"Liste içeriği yüklendi: {ListeAdi} - {Filmler.Count} film, {Diziler.Count} dizi.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Liste içeriği yüklenirken hata oluştu (ID: {ListeId}).", ex);
                // Kullanıcıya hata mesajı gösterilebilir.
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task FilmKaldirAsync(int filmId)
        {
            if (!_userService.IsLoggedIn || _userService.CurrentUserId == null)
            {
                await Application.Current.MainPage.DisplayAlert("Hata", "Bu işlem için giriş yapmalısınız.", "Tamam");
                return;
            }

            bool eminMisin = await Application.Current.MainPage.DisplayAlert("Onay", $"Bu filmi listeden kaldırmak istediğinize emin misiniz?", "Evet", "Hayır");
            if (!eminMisin) return;

            try
            {
                IsBusy = true;
                var sonuc = await _apiService.DeleteAsync($"api/kullanici-listeleri/{ListeId}/filmler/{filmId}/kullanici/{_userService.CurrentUserId.Value}");
                if (sonuc)
                {
                    _logger.LogInfo($"Film (ID: {filmId}) '{ListeAdi}' listesinden kaldırıldı.");
                    // Listeyi UI'da güncelle
                    var kaldirilacakFilm = Filmler.FirstOrDefault(f => f.Id == filmId);
                    if (kaldirilacakFilm != null) Filmler.Remove(kaldirilacakFilm);
                    OnPropertyChanged(nameof(FilmYok));
                }
                else
                {
                    _logger.LogWarning($"Film (ID: {filmId}) '{ListeAdi}' listesinden kaldırılamadı.");
                    await Application.Current.MainPage.DisplayAlert("Hata", "Film listeden kaldırılamadı.", "Tamam");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Film listeden kaldırılırken hata: {ex.Message}", ex);
                await Application.Current.MainPage.DisplayAlert("Hata", "Bir sorun oluştu.", "Tamam");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task DiziKaldirAsync(int diziId)
        {
            if (!_userService.IsLoggedIn || _userService.CurrentUserId == null)
            {
                await Application.Current.MainPage.DisplayAlert("Hata", "Bu işlem için giriş yapmalısınız.", "Tamam");
                return;
            }

            bool eminMisin = await Application.Current.MainPage.DisplayAlert("Onay", $"Bu diziyi listeden kaldırmak istediğinize emin misiniz?", "Evet", "Hayır");
            if (!eminMisin) return;

            try
            {
                IsBusy = true;
                var sonuc = await _apiService.DeleteAsync($"api/kullanici-listeleri/{ListeId}/diziler/{diziId}/kullanici/{_userService.CurrentUserId.Value}");
                if (sonuc)
                {
                    _logger.LogInfo($"Dizi (ID: {diziId}) '{ListeAdi}' listesinden kaldırıldı.");
                    var kaldirilacakDizi = Diziler.FirstOrDefault(d => d.Id == diziId);
                    if (kaldirilacakDizi != null) Diziler.Remove(kaldirilacakDizi);
                    OnPropertyChanged(nameof(DiziYok));
                }
                else
                {
                    _logger.LogWarning($"Dizi (ID: {diziId}) '{ListeAdi}' listesinden kaldırılamadı.");
                    await Application.Current.MainPage.DisplayAlert("Hata", "Dizi listeden kaldırılamadı.", "Tamam");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Dizi listeden kaldırılırken hata: {ex.Message}", ex);
                await Application.Current.MainPage.DisplayAlert("Hata", "Bir sorun oluştu.", "Tamam");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Commands
        public ICommand GeriGitCommand { get; }
    }
}