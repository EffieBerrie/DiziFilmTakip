using DiziFilmTanitim.Core.Entities;

namespace DiziFilmTanitim.Api.Models
{
    // Ortak Response Modelleri
    public record CommonTurResponseModel(int Id, string Ad);
    public record CommonOyuncuResponseModel(int Id, string AdSoyad, DateTime? DogumTarihi, string? Biyografi, string? FotografDosyaAdi);
    public record CommonYonetmenResponseModel(int Id, string AdSoyad, DateTime? DogumTarihi, string? Biyografi, string? FotografDosyaAdi);

    // Basit response modelleri (film ve dizi endpoint'lerinde kullanım için)
    public record SimpleTurResponseModel(int Id, string Ad);
    public record SimpleOyuncuResponseModel(int Id, string AdSoyad);
    public record SimpleYonetmenResponseModel(int Id, string AdSoyad);

    // API Response Wrapper
    public record CommonApiResponseModel(string Message, bool Success = true);
    public record CommonApiErrorResponseModel(string Message, bool Success = false);
}