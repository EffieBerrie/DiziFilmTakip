using System.Windows.Input;
using DiziFilmTanitim.MAUI.Services;
using DiziFilmTanitim.MAUI.Models;

namespace DiziFilmTanitim.MAUI.ViewModels
{
    public class FilmDetayViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private readonly IUserService _userService;
        private readonly ILoggingService _logger;
        private FilmDetayModel _film;
        private bool _veriYuklendi;
        private int _filmId;
        private double _ortalamaPuan = 0;
        private int _kullaniciPuani = 0;
        private int _seciliPuan = 0;
        private bool _kullaniciGirisYapti = false;

        // İster 16: DatePicker & TimePicker için property'ler
        private DateTime _izlemeTarihi = DateTime.Today;
        private TimeSpan _izlemeSaati = new TimeSpan(20, 0, 0); // Varsayılan 20:00
        private bool _hatirlatmaAktif = false;
        private bool _izlemePlaniVarMi = false;
        private string _izlemePlaniMetni = string.Empty;

        public FilmDetayViewModel(IApiService apiService, IUserService userService, ILoggingService logger)
        {
            _apiService = apiService;
            _userService = userService;
            _logger = logger;
            Title = "Film Detayı";
            _film = new FilmDetayModel();

            // Commands
            GeriGitCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
            PuanVerCommand = new Command<string>(async (puan) => await PuanVerAsync(puan), (puan) => KullaniciGirisYapti);
            GirisYapCommand = new Command(async () => await Shell.Current.GoToAsync("//GirisSayfasi"));
            ListeyeEkleCommand = new Command(async () => await ListeyeEkleAsync(), () => KullaniciGirisYapti);

            // İster 16: İzleme Planlama Command'i
            IzlemePlaniKaydetCommand = new Command(async () => await IzlemePlaniKaydetAsync(), () => KullaniciGirisYapti);

            // Kullanıcı durumunu kontrol et
            KullaniciDurumKontrol();
        }

        // Properties
        public FilmDetayModel Film
        {
            get => _film;
            set => SetProperty(ref _film, value);
        }

        public bool VeriYuklendi
        {
            get => _veriYuklendi;
            set => SetProperty(ref _veriYuklendi, value);
        }

        public int FilmId
        {
            get => _filmId;
            set
            {
                if (SetProperty(ref _filmId, value))
                {
                    _ = Task.Run(async () => await FilmDetayiYukleAsync());
                }
            }
        }

        public double OrtalamaPuan
        {
            get => _ortalamaPuan;
            set => SetProperty(ref _ortalamaPuan, value);
        }

        public int KullaniciPuani
        {
            get => _kullaniciPuani;
            set => SetProperty(ref _kullaniciPuani, value);
        }

        public int SeciliPuan
        {
            get => _seciliPuan;
            set => SetProperty(ref _seciliPuan, value);
        }

        public bool KullaniciGirisYapti
        {
            get => _kullaniciGirisYapti;
            set
            {
                if (SetProperty(ref _kullaniciGirisYapti, value))
                {
                    ((Command)PuanVerCommand).ChangeCanExecute();
                    ((Command)ListeyeEkleCommand).ChangeCanExecute();
                    ((Command)IzlemePlaniKaydetCommand).ChangeCanExecute(); // İster 16
                    OnPropertyChanged(nameof(KullaniciGirisYapmadi));
                }
            }
        }

        public bool KullaniciGirisYapmadi => !KullaniciGirisYapti;

        // İster 16: DatePicker & TimePicker Public Property'ler
        public DateTime IzlemeTarihi
        {
            get => _izlemeTarihi;
            set => SetProperty(ref _izlemeTarihi, value);
        }

        public TimeSpan IzlemeSaati
        {
            get => _izlemeSaati;
            set => SetProperty(ref _izlemeSaati, value);
        }

        public bool HatirlatmaAktif
        {
            get => _hatirlatmaAktif;
            set => SetProperty(ref _hatirlatmaAktif, value);
        }

        public bool IzlemePlaniVarMi
        {
            get => _izlemePlaniVarMi;
            set => SetProperty(ref _izlemePlaniVarMi, value);
        }

        public string IzlemePlaniMetni
        {
            get => _izlemePlaniMetni;
            set => SetProperty(ref _izlemePlaniMetni, value);
        }

        // Formatted properties for UI
        public string OrtalamaPuanText => OrtalamaPuan > 0 ? $"★ {OrtalamaPuan:F1}" : "Henüz puanlanmamış";
        public string KullaniciPuaniText => KullaniciPuani > 0 ? $"Puanınız: {KullaniciPuani} ★" : "Henüz puanlamadınız";

        // Commands
        public ICommand GeriGitCommand { get; }
        public ICommand PuanVerCommand { get; }
        public ICommand GirisYapCommand { get; }
        public ICommand ListeyeEkleCommand { get; }
        public ICommand IzlemePlaniKaydetCommand { get; }

        private async Task FilmDetayiYukleAsync()
        {
            KullaniciDurumKontrol();
            try
            {
                IsBusy = true;
                VeriYuklendi = false;

                _logger.LogDebug($"Film detayı yükleniyor, ID: {FilmId}");

                var filmResponse = await _apiService.GetAsync<FilmApiResponse>($"api/filmler/{FilmId}");

                if (filmResponse != null)
                {
                    var filmDetay = new FilmDetayModel
                    {
                        Id = filmResponse.Id,
                        Ad = filmResponse.Ad,
                        YapimYili = filmResponse.YapimYili,
                        Ozet = filmResponse.Ozet,
                        AfisDosyaAdi = filmResponse.AfisDosyaAdi,
                        AfisUrl = !string.IsNullOrEmpty(filmResponse.AfisDosyaAdi) ? $"http://localhost:5097/uploads/afisler/{filmResponse.AfisDosyaAdi}" : null,
                        SureDakika = filmResponse.SureDakika,
                        YonetmenAdi = filmResponse.Yonetmen?.AdSoyad ?? "Bilinmiyor",
                        TurlerText = filmResponse.Turler?.Any() == true ? string.Join(", ", filmResponse.Turler.Select(t => t.Ad)) : "Tür belirtilmemiş",
                        OyuncularText = filmResponse.Oyuncular?.Any() == true ? string.Join(", ", filmResponse.Oyuncular.Select(o => o.AdSoyad)) : "Oyuncu belirtilmemiş"
                    };

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Film = filmDetay;
                        VeriYuklendi = true;
                    });

                    // Puanlama verilerini yükle
                    await PuanlamaBilgileriniYukleAsync();

                    _logger.LogDebug($"Film detayı yüklendi: {filmResponse.Ad}");
                }
                else
                {
                    _logger.LogDebug("Film detayı bulunamadı");
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        VeriYuklendi = true;
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Film detayı yükleme hatası: {ex.Message}", ex);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    VeriYuklendi = true;
                });
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
                // Ortalama puanı yükle
                var ortalamaPuanResponse = await _apiService.GetAsync<OrtalamaPuanResponse>($"api/filmler/{FilmId}/ortalama-puan");
                if (ortalamaPuanResponse != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        OrtalamaPuan = ortalamaPuanResponse.OrtalamaPuan;
                        OnPropertyChanged(nameof(OrtalamaPuanText));
                    });
                }

                // Kullanıcının puanını yükle (eğer giriş yapmışsa)
                if (KullaniciGirisYapti)
                {
                    var kullaniciId = _userService.CurrentUserId;
                    if (kullaniciId.HasValue)
                    {
                        var kullaniciPuanResponse = await _apiService.GetAsync<KullaniciPuanResponse>($"api/filmler/{FilmId}/kullanici/{kullaniciId.Value}/puan");
                        if (kullaniciPuanResponse != null)
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                KullaniciPuani = kullaniciPuanResponse.Puan;
                                SeciliPuan = kullaniciPuanResponse.Puan;
                                OnPropertyChanged(nameof(KullaniciPuaniText));
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Puanlama bilgileri yükleme hatası: {ex.Message}", ex);
            }
        }

        private async Task PuanVerAsync(string puanStr)
        {
            if (!int.TryParse(puanStr, out int puan) || puan < 1 || puan > 5)
                return;

            if (!KullaniciGirisYapti)
            {
                await Application.Current.MainPage.DisplayAlert("Uyarı", "Puan vermek için giriş yapmalısınız.", "Tamam");
                return;
            }

            var kullaniciId = _userService.CurrentUserId;
            if (!kullaniciId.HasValue)
                return;

            try
            {
                IsBusy = true;

                var puanlamaRequest = new FilmPuanlamaRequest { Puan = puan };
                await _apiService.PuanlaAsync(FilmId, kullaniciId.Value, puan);

                // Puanlama başarılı - UI'ı güncelle
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    KullaniciPuani = puan;
                    SeciliPuan = puan;
                    OnPropertyChanged(nameof(KullaniciPuaniText));
                });

                // Ortalama puanı yeniden yükle
                await PuanlamaBilgileriniYukleAsync();

                await Application.Current.MainPage.DisplayAlert("Başarılı", $"Filme {puan} puan verdiniz.", "Tamam");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Puanlama hatası: {ex.Message}", ex);
                await Application.Current.MainPage.DisplayAlert("Hata", "Puanlama sırasında hata oluştu.", "Tamam");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void KullaniciDurumKontrol()
        {
            KullaniciGirisYapti = _userService.IsLoggedIn;
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

                // Filme listeye ekleme API çağrısı
                IsBusy = true;
                var basarili = await _apiService.AddFilmToListeAsync(liste.Id, _userService.CurrentUserId.Value, FilmId);

                if (basarili)
                {
                    await Application.Current.MainPage.DisplayAlert("Başarılı", $"'{Film.Ad}' filmi '{liste.ListeAdi}' listesine eklendi!", "Tamam");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Hata", "Film listeye eklenirken bir sorun oluştu. Film zaten listede olabilir.", "Tamam");
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

        // İster 16: İzleme Planlama Kaydetme
        private async Task IzlemePlaniKaydetAsync()
        {
            if (!KullaniciGirisYapti)
            {
                await Application.Current.MainPage.DisplayAlert("Uyarı", "İzleme planı kaydetmek için giriş yapmalısınız.", "Tamam");
                return;
            }

            try
            {
                // Geçmiş tarih kontrolü
                var planlananZaman = IzlemeTarihi.Date.Add(IzlemeSaati);
                if (planlananZaman <= DateTime.Now)
                {
                    await Application.Current.MainPage.DisplayAlert("Uyarı", "İzleme planı gelecek bir tarih ve saat için oluşturulmalıdır.", "Tamam");
                    return;
                }

                IsBusy = true;

                // İzleme planını kaydet (simülasyon - gerçek uygulamada API'ye kaydedilir)
                var planMetni = $"{IzlemeTarihi:dd/MM/yyyy} - {IzlemeSaati:hh\\:mm}";
                if (HatirlatmaAktif)
                {
                    planMetni += " (Hatırlatma aktif)";
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    IzlemePlaniVarMi = true;
                    IzlemePlaniMetni = planMetni;
                });

                await Application.Current.MainPage.DisplayAlert(
                    "Başarılı",
                    $"'{Film.Ad}' filmi için izleme planınız kaydedildi!\n\nTarih: {IzlemeTarihi:dd/MM/yyyy}\nSaat: {IzlemeSaati:hh\\:mm}" +
                    (HatirlatmaAktif ? "\n\nHatırlatma aktif edildi." : ""),
                    "Tamam");
            }
            catch (Exception ex)
            {
                _logger.LogError($"İzleme planı kaydetme hatası: {ex.Message}", ex);
                await Application.Current.MainPage.DisplayAlert("Hata", "İzleme planı kaydedilirken bir hata oluştu.", "Tamam");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    // Film detay model
    public class FilmDetayModel
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
        public bool OzetVarMi => !string.IsNullOrEmpty(Ozet);
    }
}