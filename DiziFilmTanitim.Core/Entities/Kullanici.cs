using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace DiziFilmTanitim.Core.Entities
{
    public class Kullanici
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string KullaniciAdi { get; set; } = null!;

        [Required]
        public string Sifre { get; set; } = null!; // Şifre düz metin olarak saklanacak

        [MaxLength(150)]
        public string? Email { get; set; }

        // Kullanıcının verdiği film puanları
        public ICollection<KullaniciFilmPuan> FilmPuanlari { get; set; } = new List<KullaniciFilmPuan>();

        // Kullanıcının verdiği dizi puanları
        public ICollection<KullaniciDiziPuan> DiziPuanlari { get; set; } = new List<KullaniciDiziPuan>();

        // Kullanıcının oluşturduğu listeler
        public ICollection<KullaniciListesi> KullaniciListeleri { get; set; } = new List<KullaniciListesi>();
    }
}