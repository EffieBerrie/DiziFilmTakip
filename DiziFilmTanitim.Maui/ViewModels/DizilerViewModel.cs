using System.Collections.ObjectModel;
using System.Windows.Input;
using DiziFilmTanitim.MAUI.Services;
using DiziFilmTanitim.MAUI.Models; // DiziApiResponse ve DiziItemViewModel için
using DiziFilmTanitim.Core.Entities; // DiziDurumu enum için eklendi

namespace DiziFilmTanitim.MAUI.ViewModels
{
    public class DizilerViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private readonly ILoggingService _logger;
        private ObservableCollection<DiziItemViewModel> _diziler;
        private bool _veriYuklendi;

        public DizilerViewModel(IApiService apiService, ILoggingService logger)
        {
            _apiService = apiService;
            _logger = logger;
            Title = "Diziler";
            _diziler = new ObservableCollection<DiziItemViewModel>();

            // Commands
            DiziSecCommand = new Command<DiziItemViewModel>(async (dizi) => await DiziDetayinaGitAsync(dizi));

            // Sayfa yüklenirken dizileri çek
            _ = Task.Run(async () => await DizileriYukleAsync());
        }

        // Properties
        public ObservableCollection<DiziItemViewModel> Diziler
        {
            get => _diziler;
            set => SetProperty(ref _diziler, value);
        }

        public bool VeriYuklendi
        {
            get => _veriYuklendi;
            set => SetProperty(ref _veriYuklendi, value);
        }

        public bool VeriYok => VeriYuklendi && !Diziler.Any();

        // Commands
        public ICommand DiziSecCommand { get; }

        private string GetDiziDurumuText(DiziDurumu durum)
        {
            return durum switch
            {
                DiziDurumu.Duyuruldu => "Duyuruldu",
                DiziDurumu.DevamEdiyor => "Devam Ediyor",
                DiziDurumu.Tamamlandi => "Tamamlandı",
                DiziDurumu.IptalEdildi => "İptal Edildi",
                DiziDurumu.AraVerdi => "Ara Verdi",
                DiziDurumu.Bilinmiyor => "Bilinmiyor",
                _ => "Bilinmiyor"
            };
        }

        private async Task DizileriYukleAsync()
        {
            try
            {
                IsBusy = true;
                VeriYuklendi = false;

                _logger.LogDebug("Diziler yükleniyor...");

                var apiResponse = await _apiService.GetAsync<ApiResponse<List<DiziApiResponse>>>("api/diziler");

                _logger.LogDebug($"API Response: Success={apiResponse?.Success}, Data Count={apiResponse?.Data?.Count ?? 0}");

                if (apiResponse?.Success == true && apiResponse.Data != null && apiResponse.Data.Count > 0)
                {
                    var diziViewModels = apiResponse.Data.Select(d => new DiziItemViewModel
                    {
                        Id = d.Id,
                        Ad = d.Ad,
                        YapimYili = d.YapimYili,
                        Ozet = d.Ozet,
                        AfisDosyaAdi = d.AfisDosyaAdi,
                        AfisUrl = !string.IsNullOrEmpty(d.AfisDosyaAdi) ? $"http://localhost:5097/uploads/afisler/{d.AfisDosyaAdi}" : null,
                        Durum = GetDiziDurumuText((DiziDurumu)d.Durum), // d.Durum (int) DiziDurumu enum'ına cast edildi ve metne çevrildi
                        YonetmenAdi = d.Yonetmen?.AdSoyad ?? "Bilinmiyor",
                        TurlerText = d.Turler?.Any() == true ? string.Join(", ", d.Turler.Select(t => t.Ad)) : "Tür belirtilmemiş",
                        OyuncularText = d.Oyuncular?.Any() == true ? string.Join(", ", d.Oyuncular.Take(3).Select(o => o.AdSoyad)) : "Oyuncu belirtilmemiş"
                    }).ToList();

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Diziler.Clear();
                        foreach (var dizi in diziViewModels)
                        {
                            Diziler.Add(dizi);
                        }
                        VeriYuklendi = true;
                        OnPropertyChanged(nameof(VeriYok));
                    });

                    _logger.LogDebug($"{apiResponse.Data.Count} dizi yüklendi");
                }
                else
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Diziler.Clear();
                        VeriYuklendi = true;
                        OnPropertyChanged(nameof(VeriYok));
                    });
                    _logger.LogDebug("Hiç dizi bulunamadı");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Dizi yükleme hatası: {ex.Message}", ex);
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

        private async Task DiziDetayinaGitAsync(DiziItemViewModel? dizi)
        {
            if (dizi == null) return;

            try
            {
                _logger.LogDebug($"Dizi detayına gidiliyor: {dizi.Ad} (ID: {dizi.Id})");
                await Shell.Current.GoToAsync($"DiziDetay?diziId={dizi.Id}");
                _logger.LogDebug("Navigation başarılı");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Dizi detay navigation hatası: {ex.Message}", ex);
                // Kullanıcıya bilgi ver
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current?.MainPage?.DisplayAlert("Hata", "Dizi detayı açılırken bir hata oluştu.", "Tamam");
                });
            }
        }
    }

    // Dizi item ViewModel
    // TODO: DiziItemViewModel modelini ApiResponseModels.cs veya ayrı bir dosyaya taşı
    public class DiziItemViewModel
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
        public int? YapimYili { get; set; }
        public string? Ozet { get; set; }
        public string? AfisDosyaAdi { get; set; }
        public string? AfisUrl { get; set; }
        public string? Durum { get; set; } // Bu string olarak kalacak, GetDiziDurumuText ile doldurulacak
        public string YonetmenAdi { get; set; } = string.Empty;
        public string TurlerText { get; set; } = string.Empty;
        public string OyuncularText { get; set; } = string.Empty;

        // UI için formatted properties
        public string YapimYiliText => YapimYili?.ToString() ?? "—";
        public string KisaOzet => string.IsNullOrEmpty(Ozet) ? "Açıklama bulunmuyor." :
                                  (Ozet.Length > 100 ? Ozet.Substring(0, 100) + "..." : Ozet);
    }
}