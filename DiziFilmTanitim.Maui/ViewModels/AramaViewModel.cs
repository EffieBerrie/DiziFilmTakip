using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using DiziFilmTanitim.MAUI.Models;
using DiziFilmTanitim.MAUI.Services;

namespace DiziFilmTanitim.MAUI.ViewModels;

public class AramaViewModel : BaseViewModel
{
    private readonly IApiService _apiService;

    private string _aramaMetni = string.Empty;
    private bool _filmlerSecili = true;
    private bool _dizilerSecili = true;
    private bool _aramaYapildi = false;
    private bool _sonucBulunamadi = false;

    // İster 15: Picker için yeni property'ler
    private TurResponse? _seciliTur;
    private string? _seciliYapimYili;
    private string _seciliSiralamaTuru = "Ad (A-Z)";

    // İster 17: RadioButton için property'ler
    private bool _siralamaAdAZ = true; // Varsayılan seçili
    private bool _siralamaAdZA = false;
    private bool _siralamaYilYeni = false;
    private bool _siralamaYilEski = false;

    public AramaViewModel(IApiService apiService)
    {
        _apiService = apiService;

        // Commands
        AramaYapCommand = new Command(async () => await AramaYapAsync(), () => !IsBusy);
        FilmDetayinaGitCommand = new Command<FilmListItemResponse>(async (film) => await FilmDetayinaGitAsync(film));
        DiziDetayinaGitCommand = new Command<DiziListItemResponse>(async (dizi) => await DiziDetayinaGitAsync(dizi));

        // İster 17: Sıralama seçimi command'i
        SiralamaSecCommand = new Command<string>((siralamaTuru) => SiralamaSecildi(siralamaTuru));

        // Collections
        Filmler = new ObservableCollection<FilmListItemResponse>();
        Diziler = new ObservableCollection<DiziListItemResponse>();

        // İster 15: Picker için koleksiyonlar
        Turler = new ObservableCollection<TurResponse>();
        YapimYillari = new ObservableCollection<string>();
        SiralamaTurleri = new ObservableCollection<string>
        {
            "Ad (A-Z)",
            "Ad (Z-A)",
            "Yapım Yılı (Yeni-Eski)",
            "Yapım Yılı (Eski-Yeni)"
        };

        // Başlangıçta verileri yükle
        _ = Task.Run(async () => await VerileriYukleAsync());
    }

    // Properties
    public string AramaMetni
    {
        get => _aramaMetni;
        set
        {
            if (SetProperty(ref _aramaMetni, value))
            {
                ((Command)AramaYapCommand).ChangeCanExecute();
            }
        }
    }

    public bool FilmlerSecili
    {
        get => _filmlerSecili;
        set => SetProperty(ref _filmlerSecili, value);
    }

    public bool DizilerSecili
    {
        get => _dizilerSecili;
        set => SetProperty(ref _dizilerSecili, value);
    }

    public bool AramaYapildi
    {
        get => _aramaYapildi;
        set => SetProperty(ref _aramaYapildi, value);
    }

    public bool SonucBulunamadi
    {
        get => _sonucBulunamadi;
        set => SetProperty(ref _sonucBulunamadi, value);
    }

    // İster 15: Picker Property'leri
    public TurResponse? SeciliTur
    {
        get => _seciliTur;
        set => SetProperty(ref _seciliTur, value);
    }

    public string? SeciliYapimYili
    {
        get => _seciliYapimYili;
        set => SetProperty(ref _seciliYapimYili, value);
    }

    public string SeciliSiralamaTuru
    {
        get => _seciliSiralamaTuru;
        set => SetProperty(ref _seciliSiralamaTuru, value);
    }

    // İster 17: RadioButton için property'ler
    public bool SiralamaAdAZ
    {
        get => _siralamaAdAZ;
        set => SetProperty(ref _siralamaAdAZ, value);
    }

    public bool SiralamaAdZA
    {
        get => _siralamaAdZA;
        set => SetProperty(ref _siralamaAdZA, value);
    }

    public bool SiralamaYilYeni
    {
        get => _siralamaYilYeni;
        set => SetProperty(ref _siralamaYilYeni, value);
    }

    public bool SiralamaYilEski
    {
        get => _siralamaYilEski;
        set => SetProperty(ref _siralamaYilEski, value);
    }

    public bool FilmlerVarMi => Filmler?.Count > 0;
    public bool DizilerVarMi => Diziler?.Count > 0;

    // Collections
    public ObservableCollection<FilmListItemResponse> Filmler { get; }
    public ObservableCollection<DiziListItemResponse> Diziler { get; }

    // İster 15: Picker için koleksiyonlar
    public ObservableCollection<TurResponse> Turler { get; }
    public ObservableCollection<string> YapimYillari { get; }
    public ObservableCollection<string> SiralamaTurleri { get; }

    // Commands
    public ICommand AramaYapCommand { get; }
    public ICommand FilmDetayinaGitCommand { get; }
    public ICommand DiziDetayinaGitCommand { get; }

    // İster 17: Sıralama seçimi command'i
    public ICommand SiralamaSecCommand { get; }

    // İster 15: Verileri yükleme metodu
    private async Task VerileriYukleAsync()
    {
        try
        {
            // Türleri yükle
            var turler = await _apiService.GetAsync<List<TurResponse>>("api/turler");
            if (turler != null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Turler.Clear();
                    Turler.Add(new TurResponse { Id = 0, Ad = "Tüm Türler" }); // Varsayılan seçenek
                    foreach (var tur in turler.OrderBy(t => t.Ad))
                    {
                        Turler.Add(tur);
                    }
                    SeciliTur = Turler.FirstOrDefault(); // İlk seçeneği seç
                });
            }

            // Yapım yıllarını yükle (son 50 yıl)
            var simdikiYil = DateTime.Now.Year;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                YapimYillari.Clear();
                YapimYillari.Add("Tüm Yıllar"); // "Tüm Yıllar" için
                for (int yil = simdikiYil; yil >= simdikiYil - 50; yil--)
                {
                    YapimYillari.Add(yil.ToString());
                }
                SeciliYapimYili = YapimYillari.FirstOrDefault(); // İlk seçeneği seç
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Veriler yüklenirken hata: {ex.Message}");
        }
    }

    private async Task AramaYapAsync()
    {
        if (string.IsNullOrWhiteSpace(AramaMetni) || AramaMetni.Length < 2)
        {
            await Shell.Current.DisplayAlert("Uyarı", "En az 2 karakter girin.", "Tamam");
            return;
        }

        try
        {
            IsBusy = true;

            // Collections'ları temizle
            Filmler.Clear();
            Diziler.Clear();

            var aramaTasks = new List<Task>();

            // Film arama
            if (FilmlerSecili)
            {
                aramaTasks.Add(FilmleriAraAsync());
            }

            // Dizi arama
            if (DizilerSecili)
            {
                aramaTasks.Add(DizileriAraAsync());
            }

            // Paralel arama yap
            await Task.WhenAll(aramaTasks);

            // İster 15: Sıralama uygula
            SiralaSonuclari();

            // Sonuçları kontrol et
            AramaYapildi = true;
            SonucBulunamadi = !FilmlerVarMi && !DizilerVarMi;

            // UI güncellemelerini bildir
            OnPropertyChanged(nameof(FilmlerVarMi));
            OnPropertyChanged(nameof(DizilerVarMi));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Hata", "Arama sırasında bir hata oluştu: " + ex.Message, "Tamam");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // İster 15: Sıralama metodu
    private void SiralaSonuclari()
    {
        if (string.IsNullOrEmpty(SeciliSiralamaTuru)) return;

        // Film sıralama
        var filmlerSirali = SeciliSiralamaTuru switch
        {
            "Ad (A-Z)" => Filmler.OrderBy(f => f.Ad).ToList(),
            "Ad (Z-A)" => Filmler.OrderByDescending(f => f.Ad).ToList(),
            "Yapım Yılı (Yeni-Eski)" => Filmler.OrderByDescending(f => f.YapimYili ?? 0).ToList(),
            "Yapım Yılı (Eski-Yeni)" => Filmler.OrderBy(f => f.YapimYili ?? 0).ToList(),
            _ => Filmler.ToList()
        };

        // Dizi sıralama
        var dizilerSirali = SeciliSiralamaTuru switch
        {
            "Ad (A-Z)" => Diziler.OrderBy(d => d.Ad).ToList(),
            "Ad (Z-A)" => Diziler.OrderByDescending(d => d.Ad).ToList(),
            "Yapım Yılı (Yeni-Eski)" => Diziler.OrderByDescending(d => d.YapimYili ?? 0).ToList(),
            "Yapım Yılı (Eski-Yeni)" => Diziler.OrderBy(d => d.YapimYili ?? 0).ToList(),
            _ => Diziler.ToList()
        };

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Filmler.Clear();
            foreach (var film in filmlerSirali)
            {
                Filmler.Add(film);
            }

            Diziler.Clear();
            foreach (var dizi in dizilerSirali)
            {
                Diziler.Add(dizi);
            }
        });
    }

    private async Task FilmleriAraAsync()
    {
        try
        {
            // İster 15: Gelişmiş filtreleme parametreleri
            var url = $"/api/filmler/ara?ad={Uri.EscapeDataString(AramaMetni)}";

            if (SeciliTur?.Id > 0)
                url += $"&turId={SeciliTur.Id}";

            if (!string.IsNullOrEmpty(SeciliYapimYili) && SeciliYapimYili != "Tüm Yıllar" && int.TryParse(SeciliYapimYili, out int yil))
                url += $"&yapimYili={yil}";

            System.Diagnostics.Debug.WriteLine($"Film arama URL: {url}");

            var response = await _apiService.GetAsync<List<FilmListItemResponse>>(url);

            System.Diagnostics.Debug.WriteLine($"Film arama response count: {response?.Count ?? 0}");

            if (response != null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    foreach (var film in response)
                    {
                        System.Diagnostics.Debug.WriteLine($"Film eklendi: {film.Ad}");
                        Filmler.Add(film);
                    }
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Film arama hatası: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    private async Task DizileriAraAsync()
    {
        try
        {
            // İster 15: Gelişmiş filtreleme parametreleri
            var url = $"/api/diziler/ara?ad={Uri.EscapeDataString(AramaMetni)}";

            if (SeciliTur?.Id > 0)
                url += $"&turId={SeciliTur.Id}";

            if (!string.IsNullOrEmpty(SeciliYapimYili) && SeciliYapimYili != "Tüm Yıllar" && int.TryParse(SeciliYapimYili, out int yil))
                url += $"&yapimYili={yil}";

            System.Diagnostics.Debug.WriteLine($"Dizi arama URL: {url}");

            var response = await _apiService.GetAsync<List<DiziListItemResponse>>(url);

            System.Diagnostics.Debug.WriteLine($"Dizi arama response count: {response?.Count ?? 0}");

            if (response != null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    foreach (var dizi in response)
                    {
                        System.Diagnostics.Debug.WriteLine($"Dizi eklendi: {dizi.Ad}");
                        Diziler.Add(dizi);
                    }
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Dizi arama hatası: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    private async Task FilmDetayinaGitAsync(FilmListItemResponse film)
    {
        if (film == null) return;

        try
        {
            System.Diagnostics.Debug.WriteLine($"Film detayına gidiliyor: {film.Ad} (ID: {film.Id})");
            await Shell.Current.GoToAsync($"FilmDetay?filmId={film.Id}");
            System.Diagnostics.Debug.WriteLine("Film navigation başarılı");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Film detay navigation hatası: {ex.Message}");
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Application.Current?.MainPage?.DisplayAlert("Hata", "Film detayı açılırken bir hata oluştu.", "Tamam");
            });
        }
    }

    private async Task DiziDetayinaGitAsync(DiziListItemResponse dizi)
    {
        if (dizi == null) return;

        try
        {
            System.Diagnostics.Debug.WriteLine($"Dizi detayına gidiliyor: {dizi.Ad} (ID: {dizi.Id})");
            await Shell.Current.GoToAsync($"DiziDetay?diziId={dizi.Id}");
            System.Diagnostics.Debug.WriteLine("Dizi navigation başarılı");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Dizi detay navigation hatası: {ex.Message}");
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Application.Current?.MainPage?.DisplayAlert("Hata", "Dizi detayı açılırken bir hata oluştu.", "Tamam");
            });
        }
    }

    // İster 17: Sıralama seçimi metodu
    private void SiralamaSecildi(string siralamaTuru)
    {
        // Önce tüm seçimleri sıfırla
        SiralamaAdAZ = false;
        SiralamaAdZA = false;
        SiralamaYilYeni = false;
        SiralamaYilEski = false;

        // Seçilen sıralamayı aktif yap
        switch (siralamaTuru)
        {
            case "AdAZ":
                SiralamaAdAZ = true;
                SeciliSiralamaTuru = "Ad (A-Z)";
                break;
            case "AdZA":
                SiralamaAdZA = true;
                SeciliSiralamaTuru = "Ad (Z-A)";
                break;
            case "YilYeni":
                SiralamaYilYeni = true;
                SeciliSiralamaTuru = "Yapım Yılı (Yeni-Eski)";
                break;
            case "YilEski":
                SiralamaYilEski = true;
                SeciliSiralamaTuru = "Yapım Yılı (Eski-Yeni)";
                break;
        }

        // Eğer arama yapılmışsa sonuçları yeniden sırala
        if (AramaYapildi)
        {
            SiralaSonuclari();
        }
    }
}