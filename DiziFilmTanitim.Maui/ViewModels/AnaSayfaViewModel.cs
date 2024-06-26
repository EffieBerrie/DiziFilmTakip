using System.Collections.ObjectModel;
using System.Windows.Input;
using DiziFilmTanitim.MAUI.Services;
using DiziFilmTanitim.MAUI.Models;

namespace DiziFilmTanitim.MAUI.ViewModels
{
    // Ana sayfa için basit item view model'leri
    public class AnaSayfaFilmItem
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
        public int? YapimYili { get; set; }
        public string? AfisDosyaAdi { get; set; }
        public string YapimYiliText => YapimYili?.ToString() ?? "—";
        public string AfisUrl => !string.IsNullOrEmpty(AfisDosyaAdi)
            ? $"http://localhost:5097/uploads/afisler/{AfisDosyaAdi}"
            : "https://via.placeholder.com/200x300/E3F2FD/2196F3?text=Film";
    }

    public class AnaSayfaDiziItem
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
        public int? YapimYili { get; set; }
        public string? AfisDosyaAdi { get; set; }
        public int Durum { get; set; }
        public string YapimYiliText => YapimYili?.ToString() ?? "—";
        public string DurumText => Durum switch
        {
            0 => "Bilinmiyor",
            1 => "Duyuruldu",
            2 => "Devam Ediyor",
            3 => "Tamamlandı",
            4 => "İptal Edildi",
            5 => "Ara Verdi",
            _ => "Bilinmiyor"
        };
        public string AfisUrl => !string.IsNullOrEmpty(AfisDosyaAdi)
            ? $"http://localhost:5097/uploads/afisler/{AfisDosyaAdi}"
            : "https://via.placeholder.com/200x300/E3F2FD/2196F3?text=Dizi";
    }

    public class AnaSayfaViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private readonly ILoggingService _logger;
        private ObservableCollection<AnaSayfaFilmItem> _filmler = new();
        private ObservableCollection<AnaSayfaDiziItem> _diziler = new();

        public AnaSayfaViewModel(IApiService apiService, ILoggingService logger)
        {
            _apiService = apiService;
            _logger = logger;
            Title = "Ana Sayfa";

            // Commands
            FilmDetayinaGitCommand = new Command<AnaSayfaFilmItem>(async (film) => await FilmDetayinaGitAsync(film));
            DiziDetayinaGitCommand = new Command<AnaSayfaDiziItem>(async (dizi) => await DiziDetayinaGitAsync(dizi));
            TumFilmlereGitCommand = new Command(async () => await Shell.Current.GoToAsync("//Filmler"));
            TumDizilereGitCommand = new Command(async () => await Shell.Current.GoToAsync("//Diziler"));

            // Sayfa yüklenirken verileri çek
            _ = Task.Run(async () => await VeriCekAsync());
        }

        // Collections
        public ObservableCollection<AnaSayfaFilmItem> Filmler
        {
            get => _filmler;
            set => SetProperty(ref _filmler, value);
        }

        public ObservableCollection<AnaSayfaDiziItem> Diziler
        {
            get => _diziler;
            set => SetProperty(ref _diziler, value);
        }

        // Properties
        public bool FilmlerVarMi => Filmler?.Count > 0;
        public bool DizilerVarMi => Diziler?.Count > 0;

        // Commands
        public ICommand FilmDetayinaGitCommand { get; }
        public ICommand DiziDetayinaGitCommand { get; }
        public ICommand TumFilmlereGitCommand { get; }
        public ICommand TumDizilereGitCommand { get; }

        private async Task VeriCekAsync()
        {
            try
            {
                IsBusy = true;
                _logger.LogDebug("Ana sayfa verileri yükleniyor...");

                // Paralel olarak film ve dizi verilerini çek
                var filmTask = _apiService.GetAsync<ApiResponse<List<FilmApiResponse>>>("api/filmler");
                var diziTask = _apiService.GetAsync<ApiResponse<List<DiziApiResponse>>>("api/diziler");

                // Tüm API çağrılarının tamamlanmasını bekle
                await Task.WhenAll(filmTask, diziTask);

                // Sonuçları al
                var filmResponse = await filmTask;
                var diziResponse = await diziTask;

                // UI thread'de güncelle
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    // İlk 2 filmi ekle
                    Filmler.Clear();
                    if (filmResponse?.Success == true && filmResponse.Data != null)
                    {
                        var ilkFilmler = filmResponse.Data.Take(2);
                        foreach (var film in ilkFilmler)
                        {
                            Filmler.Add(new AnaSayfaFilmItem
                            {
                                Id = film.Id,
                                Ad = film.Ad,
                                YapimYili = film.YapimYili,
                                AfisDosyaAdi = film.AfisDosyaAdi
                            });
                        }
                    }

                    // İlk 2 diziyi ekle
                    Diziler.Clear();
                    if (diziResponse?.Success == true && diziResponse.Data != null)
                    {
                        var ilkDiziler = diziResponse.Data.Take(2);
                        foreach (var dizi in ilkDiziler)
                        {
                            Diziler.Add(new AnaSayfaDiziItem
                            {
                                Id = dizi.Id,
                                Ad = dizi.Ad,
                                YapimYili = dizi.YapimYili,
                                AfisDosyaAdi = dizi.AfisDosyaAdi,
                                Durum = dizi.Durum
                            });
                        }
                    }

                    OnPropertyChanged(nameof(FilmlerVarMi));
                    OnPropertyChanged(nameof(DizilerVarMi));
                });

                _logger.LogDebug($"Ana sayfa verileri yüklendi - Film: {Filmler.Count}, Dizi: {Diziler.Count}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ana sayfa veri çekme hatası: {ex.Message}", ex);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Filmler.Clear();
                    Diziler.Clear();
                    OnPropertyChanged(nameof(FilmlerVarMi));
                    OnPropertyChanged(nameof(DizilerVarMi));
                });
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task FilmDetayinaGitAsync(AnaSayfaFilmItem film)
        {
            if (film == null) return;

            try
            {
                _logger.LogDebug($"Film detayına gidiliyor: {film.Ad} (ID: {film.Id})");
                await Shell.Current.GoToAsync($"FilmDetay?filmId={film.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Film detay navigation hatası: {ex.Message}", ex);
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current?.MainPage?.DisplayAlert("Hata", "Film detayı açılırken bir hata oluştu.", "Tamam");
                });
            }
        }

        private async Task DiziDetayinaGitAsync(AnaSayfaDiziItem dizi)
        {
            if (dizi == null) return;

            try
            {
                _logger.LogDebug($"Dizi detayına gidiliyor: {dizi.Ad} (ID: {dizi.Id})");
                await Shell.Current.GoToAsync($"DiziDetay?diziId={dizi.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Dizi detay navigation hatası: {ex.Message}", ex);
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current?.MainPage?.DisplayAlert("Hata", "Dizi detayı açılırken bir hata oluştu.", "Tamam");
                });
            }
        }
    }
}