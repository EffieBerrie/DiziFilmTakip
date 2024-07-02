using System.Collections.ObjectModel;
using System.Windows.Input;
using DiziFilmTanitim.MAUI.Services;
using DiziFilmTanitim.MAUI.Models;

namespace DiziFilmTanitim.MAUI.ViewModels
{
    public class FilmlerViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private readonly ILoggingService _logger;
        private ObservableCollection<FilmItemViewModel> _filmler;
        private bool _veriYuklendi;

        public FilmlerViewModel(IApiService apiService, ILoggingService logger)
        {
            _apiService = apiService;
            _logger = logger;
            Title = "Filmler";
            _filmler = new ObservableCollection<FilmItemViewModel>();

            // Commands
            FilmSecCommand = new Command<FilmItemViewModel>(async (film) => await FilmDetayinaGitAsync(film));

            // Sayfa yüklenirken filmleri çek
            _ = Task.Run(async () => await FilmleriYukleAsync());
        }

        // Properties
        public ObservableCollection<FilmItemViewModel> Filmler
        {
            get => _filmler;
            set => SetProperty(ref _filmler, value);
        }

        public bool VeriYuklendi
        {
            get => _veriYuklendi;
            set => SetProperty(ref _veriYuklendi, value);
        }

        public bool VeriYok => VeriYuklendi && !Filmler.Any();

        // Commands
        public ICommand FilmSecCommand { get; }

        private async Task FilmleriYukleAsync()
        {
            try
            {
                IsBusy = true;
                VeriYuklendi = false;

                _logger.LogDebug("Filmler yükleniyor...");

                var apiResponse = await _apiService.GetAsync<ApiResponse<List<FilmApiResponse>>>("api/filmler");

                _logger.LogDebug($"API Response: Success={apiResponse?.Success}, Data Count={apiResponse?.Data?.Count ?? 0}");

                if (apiResponse?.Success == true && apiResponse.Data != null && apiResponse.Data.Count > 0)
                {
                    var filmViewModels = apiResponse.Data.Select(f => new FilmItemViewModel
                    {
                        Id = f.Id,
                        Ad = f.Ad,
                        YapimYili = f.YapimYili,
                        Ozet = f.Ozet,
                        AfisDosyaAdi = f.AfisDosyaAdi,
                        AfisUrl = !string.IsNullOrEmpty(f.AfisDosyaAdi) ? $"http://localhost:5097/uploads/afisler/{f.AfisDosyaAdi}" : null,
                        SureDakika = f.SureDakika,
                        YonetmenAdi = f.Yonetmen?.AdSoyad ?? "Bilinmiyor",
                        TurlerText = f.Turler?.Any() == true ? string.Join(", ", f.Turler.Select(t => t.Ad)) : "Tür belirtilmemiş",
                        OyuncularText = f.Oyuncular?.Any() == true ? string.Join(", ", f.Oyuncular.Take(3).Select(o => o.AdSoyad)) : "Oyuncu belirtilmemiş"
                    }).ToList();

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Filmler.Clear();
                        foreach (var film in filmViewModels)
                        {
                            Filmler.Add(film);
                        }
                        VeriYuklendi = true;
                        OnPropertyChanged(nameof(VeriYok));
                    });

                    _logger.LogDebug($"{apiResponse.Data.Count} film yüklendi");
                }
                else
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Filmler.Clear();
                        VeriYuklendi = true;
                        OnPropertyChanged(nameof(VeriYok));
                    });
                    _logger.LogDebug("Hiç film bulunamadı");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Film yükleme hatası: {ex.Message}", ex);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    VeriYuklendi = true;
                    OnPropertyChanged(nameof(VeriYok));
                });
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task FilmDetayinaGitAsync(FilmItemViewModel? film)
        {
            if (film == null) return;

            try
            {
                _logger.LogDebug($"Film detayına gidiliyor: {film.Ad} (ID: {film.Id})");
                await Shell.Current.GoToAsync($"FilmDetay?filmId={film.Id}");
                _logger.LogDebug("Navigation başarılı");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Film detay navigation hatası: {ex.Message}", ex);
                // Kullanıcıya bilgi ver
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current?.MainPage?.DisplayAlert("Hata", "Film detayı açılırken bir hata oluştu.", "Tamam");
                });
            }
        }
    }

    // Film item ViewModel
    public class FilmItemViewModel
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
        public int? YapimYili { get; set; }
        public string? Ozet { get; set; }
        public string? AfisDosyaAdi { get; set; }
        public string? AfisUrl { get; set; }
        public int? SureDakika { get; set; }
        public string YonetmenAdi { get; set; } = string.Empty;
        public string TurlerText { get; set; } = string.Empty;
        public string OyuncularText { get; set; } = string.Empty;

        // UI için formatted properties
        public string YapimYiliText => YapimYili?.ToString() ?? "—";
        public string SureText => SureDakika.HasValue ? $"{SureDakika} dk" : "—";
        public string KisaOzet => string.IsNullOrEmpty(Ozet) ? "Açıklama bulunmuyor." :
                                  (Ozet.Length > 100 ? Ozet.Substring(0, 100) + "..." : Ozet);
    }
}