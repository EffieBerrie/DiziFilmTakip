namespace DiziFilmTanitim.MAUI.Models
{
    // Ortak API Response Model'leri
    public record CommonApiResponseModel(string Message, bool Success = true);

    public record CommonApiErrorResponseModel(string Message, bool Success = false);

    // Generic API Response Model
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }

    // Film API Response Models
    public class FilmApiResponse
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
        public int? YapimYili { get; set; }
        public string? Ozet { get; set; }
        public string? AfisDosyaAdi { get; set; }
        public int? SureDakika { get; set; }
        public SimpleYonetmenResponse? Yonetmen { get; set; }
        public List<SimpleTurResponse>? Turler { get; set; }
        public List<SimpleOyuncuResponse>? Oyuncular { get; set; }
    }

    // Dizi API Response Model (Yeni Eklendi)
    public class DiziApiResponse
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
        public int? YapimYili { get; set; }
        public string? Ozet { get; set; }
        public string? AfisDosyaAdi { get; set; }
        public int Durum { get; set; }
        public SimpleYonetmenResponse? Yonetmen { get; set; }
        public List<SimpleTurResponse>? Turler { get; set; }
        public List<SimpleOyuncuResponse>? Oyuncular { get; set; }
        // Sezonlar ve Bolumler detay sayfasında yüklenecek
    }

    // Bölüm, Sezon ve Dizi Detay için API Response Modelleri (Yeni Eklendi)
    public class BolumApiResponse
    {
        public int Id { get; set; }
        public int BolumNumarasi { get; set; }
        public string? Ad { get; set; }
        public string? Ozet { get; set; }
        public DateTime? YayinTarihi { get; set; }
        public int? SureDakika { get; set; }
    }

    public class SezonApiResponse
    {
        public int Id { get; set; }
        public int SezonNumarasi { get; set; }
        public string? Ad { get; set; }
        public DateTime? YayinTarihi { get; set; }
        public List<BolumApiResponse>? Bolumler { get; set; }
    }

    public class DiziDetayApiResponse // FilmApiResponse'a benzer ama Sezonlar içerir
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
        public int? YapimYili { get; set; }
        public string? Ozet { get; set; }
        public string? AfisDosyaAdi { get; set; }
        public int Durum { get; set; }
        public SimpleYonetmenResponse? Yonetmen { get; set; }
        public List<SimpleTurResponse>? Turler { get; set; }
        public List<SimpleOyuncuResponse>? Oyuncular { get; set; }
        public List<SezonApiResponse>? Sezonlar { get; set; }
    }

    // Backend ile uyumlu Simple Response Models
    public class SimpleYonetmenResponse
    {
        public int Id { get; set; }
        public string AdSoyad { get; set; } = string.Empty;
    }

    public class SimpleTurResponse
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
    }

    // İster 15: Picker için Tür Response Modeli
    public class TurResponse
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;

        // Picker için display text
        public override string ToString() => Ad;
    }

    public class SimpleOyuncuResponse
    {
        public int Id { get; set; }
        public string AdSoyad { get; set; } = string.Empty;
    }

    // Puanlama Response Models
    public class OrtalamaPuanResponse
    {
        public double OrtalamaPuan { get; set; }
        public int FilmId { get; set; }
    }

    public class KullaniciPuanResponse
    {
        public int KullaniciId { get; set; }
        public int FilmId { get; set; }
        public int Puan { get; set; }
    }

    // Request Models
    public class FilmPuanlamaRequest
    {
        public int Puan { get; set; }
    }

    // Dizi Puanlama Response Models (Yeni Eklendi)
    public class OrtalamaDiziPuanResponse
    {
        public double OrtalamaPuan { get; set; }
        public int DiziId { get; set; }
    }

    public class KullaniciDiziPuanResponse
    {
        public int KullaniciId { get; set; }
        public int DiziId { get; set; }
        public int Puan { get; set; }
    }

    // Authentication Response Models
    public class LoginResponse
    {
        public int Id { get; set; }
        public string KullaniciAdi { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class KayitResponse
    {
        public int Id { get; set; }
        public string KullaniciAdi { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    // Basic Response Models (Ana sayfa için)
    public class BasicFilmResponse
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
    }

    public class BasicDiziResponse
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
    }

    public class BasicKullaniciResponse
    {
        public int Id { get; set; }
        public string KullaniciAdi { get; set; } = string.Empty;
    }

    // Kullanıcı Listesi Response Model (Yeni Eklendi)
    public record KullaniciListesiResponseModel(int Id, string ListeAdi, string? Aciklama, int KullaniciId);

    // Bir Kullanıcı Listesi İçindeki Film ve Dizi Modelleri (Yeni Eklendi)
    public record ListeFilmResponseModel(int Id, string Ad, int? YapimYili, string? AfisDosyaAdi);
    public record ListeDiziResponseModel(int Id, string Ad, int? YapimYili, string? AfisDosyaAdi, int Durum); // Durum int olarak alınacak

    // Liste Oluşturma Request Modeli
    public record KullaniciListesiEkleRequest(string ListeAdi, string? Aciklama);

    // Arama Sonuçları için List Item Response Models
    public class FilmListItemResponse
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
        public int? YapimYili { get; set; }
        public string? AfisDosyaAdi { get; set; }
        public int? SureDakika { get; set; }
        public SimpleYonetmenResponse? Yonetmen { get; set; }
        public List<SimpleTurResponse>? Turler { get; set; }

        // Computed Properties
        public string YapimYiliText => YapimYili?.ToString() ?? "Bilinmiyor";
        public string SureText => SureDakika.HasValue ? $"{SureDakika} dk" : "Bilinmiyor";
        public string YonetmenAdi => Yonetmen?.AdSoyad ?? "Bilinmiyor";
        public string AfisUrl => !string.IsNullOrEmpty(AfisDosyaAdi)
            ? $"http://localhost:5097/uploads/afisler/{AfisDosyaAdi}"
            : "https://via.placeholder.com/300x450/E3F2FD/2196F3?text=Film";
    }

    public class DiziListItemResponse
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
        public int? YapimYili { get; set; }
        public string? AfisDosyaAdi { get; set; }
        public int Durum { get; set; }
        public SimpleYonetmenResponse? Yonetmen { get; set; }
        public List<SimpleTurResponse>? Turler { get; set; }

        // Computed Properties
        public string YapimYiliText => YapimYili?.ToString() ?? "Bilinmiyor";
        public string YonetmenAdi => Yonetmen?.AdSoyad ?? "Bilinmiyor";
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
            : "https://via.placeholder.com/300x450/E3F2FD/2196F3?text=Dizi";
    }
}