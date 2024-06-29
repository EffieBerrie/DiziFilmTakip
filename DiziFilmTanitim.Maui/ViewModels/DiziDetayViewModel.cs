using System.Collections.ObjectModel;
using System.Windows.Input;
using DiziFilmTanitim.MAUI.Services;
using DiziFilmTanitim.MAUI.Models;
using DiziFilmTanitim.Core.Entities;

namespace DiziFilmTanitim.MAUI.ViewModels
{
    // DiziDetayViewModel için UI katmanında kullanılacak modeller
    public class BolumItemViewModel // DiziDetayModel içinde kullanılacak
    {
        public int Id { get; set; }
        public int BolumNumarasi { get; set; }
        public string? Ad { get; set; }
        public string? Ozet { get; set; }
        public string YayinTarihiText => YayinTarihi?.ToShortDateString() ?? "-";
        public DateTime? YayinTarihi { get; set; }
        public string SureText => SureDakika.HasValue ? $"{SureDakika} dk" : "-";
        public int? SureDakika { get; set; }
    }

    public class SezonItemViewModel : BaseViewModel // BaseViewModel'dan miras alacak (IsExpanded için OnPropertyChanged)
    {
        private bool _isExpanded;
        public int Id { get; set; }
        public int SezonNumarasi { get; set; }
        public string? Ad { get; set; }
        public string YayinTarihiText => YayinTarihi?.ToShortDateString() ?? "-";
        public DateTime? YayinTarihi { get; set; }
        public ObservableCollection<BolumItemViewModel> Bolumler { get; set; } = new();
        public string BolumSayisiText => $"{Bolumler.Count} Bölüm";

        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        public ICommand ToggleExpandCommand { get; }

        public SezonItemViewModel()
        {
            ToggleExpandCommand = new Command(() => IsExpanded = !IsExpanded);
        }
    }

    public class DiziDetayModel // ViewModel'ın ana veri modeli
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
        public int? YapimYili { get; set; }
        public string? Ozet { get; set; }
        public string? AfisDosyaAdi { get; set; }
        public string? AfisUrl { get; set; }
        public string Durum { get; set; } = string.Empty;
        public string YonetmenAdi { get; set; } = string.Empty;
        public string TurlerText { get; set; } = string.Empty;
        public string OyuncularText { get; set; } = string.Empty;
        public ObservableCollection<SezonItemViewModel> Sezonlar { get; set; } = new();

        // UI için formatlanmış özellikler
        public string YapimYiliText => YapimYili?.ToString() ?? "—";
        public bool OzetVarMi => !string.IsNullOrEmpty(Ozet);
    }

    public class DiziDetayViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private readonly IUserService _userService;
        private readonly ILoggingService _logger;
        private DiziDetayModel _dizi = new();
        private bool _veriYuklendi;
        private int _diziId;
        private double _ortalamaPuan = 0;
        private int _kullaniciPuani = 0;
        // private int _seciliPuan = 0; // Puanlama UI'ı için gerekirse eklenecek

        public DiziDetayViewModel(IApiService apiService, IUserService userService, ILoggingService logger)
        {
            _apiService = apiService;
            _userService = userService;
            _logger = logger;
            Title = "Dizi Detayı";

            GeriGitCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
            PuanVerCommand = new Command<string>(async (puan) => await PuanVerAsync(puan), (puan) => KullaniciGirisYapti);
            GirisYapCommand = new Command(async () => await Shell.Current.GoToAsync("//GirisSayfasi"));
            ListeyeEkleCommand = new Command(async () => await ListeyeEkleAsync(), () => KullaniciGirisYapti);

            KullaniciDurumKontrol();
        }

        public DiziDetayModel Dizi
        {
            get => _dizi;
            set => SetProperty(ref _dizi, value);
        }

        public bool VeriYuklendi
        {
            get => _veriYuklendi;
            set => SetProperty(ref _veriYuklendi, value);
        }

        public int DiziId
        {
            get => _diziId;
            set
            {
                if (SetProperty(ref _diziId, value))
                {
                    _ = Task.Run(async () => await DiziDetayiYukleAsync());
                }
            }
        }

        public double OrtalamaPuan
        {
            get => _ortalamaPuan;
            set
            {
                if (SetProperty(ref _ortalamaPuan, value))
                    OnPropertyChanged(nameof(OrtalamaPuanText));
            }
        }

        public int KullaniciPuani
        {
            get => _kullaniciPuani;
            set
            {
                if (SetProperty(ref _kullaniciPuani, value))
                    OnPropertyChanged(nameof(KullaniciPuaniText));
            }
        }

        public bool KullaniciGirisYapti { get; private set; }
        public bool KullaniciGirisYapmadi => !KullaniciGirisYapti;

        public string OrtalamaPuanText => OrtalamaPuan > 0 ? $"★ {OrtalamaPuan:F1}" : "Henüz puanlanmamış";
        public string KullaniciPuaniText => KullaniciPuani > 0 ? $"Puanınız: {KullaniciPuani} ★" : "Henüz puanlamadınız";

        public ICommand GeriGitCommand { get; }
        public ICommand PuanVerCommand { get; }
        public ICommand GirisYapCommand { get; }
        public ICommand ListeyeEkleCommand { get; }

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

        private void KullaniciDurumKontrol()
        {
            KullaniciGirisYapti = _userService.IsLoggedIn;
            OnPropertyChanged(nameof(KullaniciGirisYapti));
            OnPropertyChanged(nameof(KullaniciGirisYapmadi));
            ((Command)PuanVerCommand).ChangeCanExecute();
            ((Command)ListeyeEkleCommand).ChangeCanExecute();
        }

        private async Task DiziDetayiYukleAsync()
        {
            KullaniciDurumKontrol();
            try
            {
                IsBusy = true;
                VeriYuklendi = false;
                _logger.LogDebug($"Dizi detayı yükleniyor, ID: {DiziId}");

                var diziResponse = await _apiService.GetAsync<DiziDetayApiResponse>($"api/diziler/{DiziId}");

                if (diziResponse != null)
                {
                    var diziDetay = new DiziDetayModel
                    {
                        Id = diziResponse.Id,
                        Ad = diziResponse.Ad,
                        YapimYili = diziResponse.YapimYili,
                        Ozet = diziResponse.Ozet,
                        AfisDosyaAdi = diziResponse.AfisDosyaAdi,
                        AfisUrl = !string.IsNullOrEmpty(diziResponse.AfisDosyaAdi) ? $"http://localhost:5097/uploads/afisler/{diziResponse.AfisDosyaAdi}" : null,
                        Durum = GetDiziDurumuText((DiziDurumu)diziResponse.Durum),
                        YonetmenAdi = diziResponse.Yonetmen?.AdSoyad ?? "Bilinmiyor",
                        TurlerText = diziResponse.Turler?.Any() == true ? string.Join(", ", diziResponse.Turler.Select(t => t.Ad)) : "Tür belirtilmemiş",
                        OyuncularText = diziResponse.Oyuncular?.Any() == true ? string.Join(", ", diziResponse.Oyuncular.Select(o => o.AdSoyad)) : "Oyuncu belirtilmemiş",
                        Sezonlar = new ObservableCollection<SezonItemViewModel>(
                            diziResponse.Sezonlar?.Select(s => new SezonItemViewModel
                            {
                                Id = s.Id,
                                SezonNumarasi = s.SezonNumarasi,
                                Ad = s.Ad,
                                YayinTarihi = s.YayinTarihi,
                                Bolumler = new ObservableCollection<BolumItemViewModel>(
                                    s.Bolumler?.Select(b => new BolumItemViewModel
                                    {
                                        Id = b.Id,
                                        BolumNumarasi = b.BolumNumarasi,
                                        Ad = b.Ad,
                                        Ozet = b.Ozet,
                                        YayinTarihi = b.YayinTarihi,
                                        SureDakika = b.SureDakika
                                    }).ToList() ?? new List<BolumItemViewModel>())
                            }).ToList() ?? new List<SezonItemViewModel>())
                    };

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Dizi = diziDetay;
                        VeriYuklendi = true;
                    });

                    await PuanlamaBilgileriniYukleAsync();
                    _logger.LogDebug($"Dizi detayı yüklendi: {diziResponse.Ad}");
                }
                else
                {
                    _logger.LogDebug("Dizi detayı bulunamadı");
                    MainThread.BeginInvokeOnMainThread(() => { VeriYuklendi = true; });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Dizi detayı yükleme hatası: {ex.Message}", ex);
                MainThread.BeginInvokeOnMainThread(() => { VeriYuklendi = true; });
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task PuanlamaBilgileriniYukleAsync()
        {
            try
            {
                var ortalamaPuanResponse = await _apiService.OrtalamaDiziPuaniGetirAsync(DiziId);
                if (ortalamaPuanResponse.HasValue)
                {
                    OrtalamaPuan = ortalamaPuanResponse.Value;
                }
                else { OrtalamaPuan = 0; } // API null dönerse veya hata olursa puanı sıfırla

                if (KullaniciGirisYapti && _userService.CurrentUserId.HasValue)
                {
                    var kullaniciPuanResponse = await _apiService.KullaniciDiziPuaniGetirAsync(DiziId, _userService.CurrentUserId.Value);
                    if (kullaniciPuanResponse.HasValue)
                    {
                        KullaniciPuani = kullaniciPuanResponse.Value;
                    }
                    else { KullaniciPuani = 0; } // API null dönerse veya hata olursa puanı sıfırla
                }
                else
                {
                    KullaniciPuani = 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Dizi puanlama bilgileri yükleme hatası: {ex.Message}", ex);
                OrtalamaPuan = 0;
                KullaniciPuani = 0;
            }
        }

        private async Task PuanVerAsync(string puanStr)
        {
            if (!int.TryParse(puanStr, out int puan) || puan < 1 || puan > 5 || !KullaniciGirisYapti || !_userService.CurrentUserId.HasValue)
            {
                if (!KullaniciGirisYapti)
                    await Application.Current.MainPage.DisplayAlert("Uyarı", "Puan vermek için giriş yapmalısınız.", "Tamam");
                return;
            }

            var kullaniciId = _userService.CurrentUserId.Value;

            try
            {
                IsBusy = true;
                bool sonuc = await _apiService.PuanlaDiziAsync(DiziId, kullaniciId, puan);

                if (sonuc)
                {
                    await PuanlamaBilgileriniYukleAsync(); // Puanları yeniden yükle
                    await Application.Current.MainPage.DisplayAlert("Başarılı", $"Diziye {puan} puan verdiniz.", "Tamam");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Hata", "Puanlama sırasında bir sorun oluştu.", "Tamam");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Diziye puan verme hatası: {ex.Message}", ex);
                await Application.Current.MainPage.DisplayAlert("Hata", "Puanlama sırasında hata oluştu.", "Tamam");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ListeyeEkleAsync()
        {
            if (!KullaniciGirisYapti || !_userService.CurrentUserId.HasValue)
            {
                await Shell.Current.GoToAsync("//GirisSayfasi");
                return;
            }

            try
            {
                // Kullanıcının listelerini getir
                var listeler = await _apiService.GetAsync<KullaniciListesiResponseModel[]>($"api/kullanici-listeleri/kullanici/{_userService.CurrentUserId.Value}");

                if (listeler == null || !listeler.Any())
                {
                    var result = await Application.Current.MainPage.DisplayAlert(
                        "Liste Yok",
                        "Henüz hiç listeniz yok. Yeni bir liste oluşturmak ister misiniz?",
                        "Evet", "Hayır");

                    if (result)
                    {
                        await Shell.Current.GoToAsync("//MainTabs/Listeler");
                    }
                    return;
                }

                // Liste seçim ekranı göster
                var listeAdlari = listeler.Select(l => l.ListeAdi).ToArray();
                var seciliListe = await Application.Current.MainPage.DisplayActionSheet(
                    "Hangi listeye eklemek istiyorsunuz?",
                    "İptal",
                    null,
                    listeAdlari);

                if (seciliListe == "İptal" || seciliListe == null)
                    return;

                var liste = listeler.FirstOrDefault(l => l.ListeAdi == seciliListe);
                if (liste == null) return;

                // Diziyi listeye ekleme API çağrısı
                IsBusy = true;
                var basarili = await _apiService.AddDiziToListeAsync(liste.Id, _userService.CurrentUserId.Value, DiziId);

                if (basarili)
                {
                    await Application.Current.MainPage.DisplayAlert("Başarılı", $"'{Dizi.Ad}' dizisi '{liste.ListeAdi}' listesine eklendi!", "Tamam");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Hata", "Dizi listeye eklenirken bir sorun oluştu. Dizi zaten listede olabilir.", "Tamam");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Listeye ekleme hatası: {ex.Message}", ex);
                await Application.Current.MainPage.DisplayAlert("Hata", "Beklenmedik bir hata oluştu.", "Tamam");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}