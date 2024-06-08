using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace DiziFilmTanitim.Core.Entities
{
    public class Film
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Ad { get; set; } = null!;

        public int? YapimYili { get; set; }

        [MaxLength(500)]
        public string? Ozet { get; set; }

        public string? AfisDosyaAdi { get; set; } // Film afişinin dosya adı (örn: poster.jpg)
        public int? SureDakika { get; set; } // Filmin süresi (dakika cinsinden)

        // Yonetmen ilişkisi
        public int? YonetmenId { get; set; } // Foreign key
        public Yonetmen? Yonetmen { get; set; } // Navigation property

        // Film-Tur çoktan-çoğa ilişkisi
        public ICollection<Tur> Turler { get; set; } = new List<Tur>();

        // Film-Oyuncu çoktan-çoğa ilişkisi
        public ICollection<Oyuncu> Oyuncular { get; set; } = new List<Oyuncu>();

        // Filme verilen kullanıcı puanları
        public ICollection<KullaniciFilmPuan> KullaniciPuanlari { get; set; } = new List<KullaniciFilmPuan>();

        // Filmin bulunduğu kullanıcı listeleri
        public ICollection<KullaniciListesi> KullaniciListeleri { get; set; } = new List<KullaniciListesi>();

        // İleride Oyuncular gibi ilişkili tablolar eklenebilir.
    }
}