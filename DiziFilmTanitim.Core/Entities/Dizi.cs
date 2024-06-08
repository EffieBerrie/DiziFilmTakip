using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace DiziFilmTanitim.Core.Entities
{
    public class Dizi
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Ad { get; set; } = null!;

        public int? YapimYili { get; set; }

        [MaxLength(1000)] // Özeti biraz daha uzun olabilir
        public string? Ozet { get; set; }

        public string? AfisDosyaAdi { get; set; } // Dizi afişinin dosya adı

        public DiziDurumu Durum { get; set; } = DiziDurumu.Bilinmiyor; // Dizinin güncel durumu

        // Yonetmen ilişkisi (Bir dizinin birden fazla yönetmeni olabilir, bu şimdilik ana yönetmen gibi düşünülebilir veya ileride çoktan-çoğa ilişki kurulabilir)
        public int? YonetmenId { get; set; } // Foreign key
        public Yonetmen? Yonetmen { get; set; } // Navigation property

        // Bir diziye ait sezonlar
        public ICollection<Sezon> Sezonlar { get; set; } = new List<Sezon>();

        // Dizi-Tur çoktan-çoğa ilişkisi
        public ICollection<Tur> Turler { get; set; } = new List<Tur>();

        // Dizi-Oyuncu çoktan-çoğa ilişkisi
        public ICollection<Oyuncu> Oyuncular { get; set; } = new List<Oyuncu>();

        // Diziye verilen kullanıcı puanları
        public ICollection<KullaniciDiziPuan> KullaniciPuanlari { get; set; } = new List<KullaniciDiziPuan>();

        // Dizinin bulunduğu kullanıcı listeleri
        public ICollection<KullaniciListesi> KullaniciListeleri { get; set; } = new List<KullaniciListesi>();

        // İleride Oyuncular gibi ilişkili tablolar eklenebilir.
    }
}