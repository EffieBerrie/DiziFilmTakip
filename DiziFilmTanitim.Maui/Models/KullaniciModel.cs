namespace DiziFilmTanitim.MAUI.Models
{
    public class KullaniciModel
    {
        public int Id { get; set; }
        public string KullaniciAdi { get; set; } = string.Empty;
        public string? Email { get; set; }

        // UI için ek özellikler
        public string DisplayName => string.IsNullOrEmpty(KullaniciAdi) ? "Misafir" : KullaniciAdi;
    }

    // API çağrıları için model'ler
    public class LoginRequest
    {
        public string KullaniciAdi { get; set; } = string.Empty;
        public string Sifre { get; set; } = string.Empty;
    }

    public class KayitRequest
    {
        public string KullaniciAdi { get; set; } = string.Empty;
        public string Eposta { get; set; } = string.Empty;
        public string Sifre { get; set; } = string.Empty;
    }
}