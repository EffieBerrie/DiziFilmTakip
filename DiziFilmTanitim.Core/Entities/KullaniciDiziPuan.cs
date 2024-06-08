using System.ComponentModel.DataAnnotations;

namespace DiziFilmTanitim.Core.Entities
{
    // Kullanıcının bir diziye verdiği puanı temsil eder.
    // Bu entity için AppDbContext içerisinde OnModelCreating altında
    // HasKey(kdp => new { kdp.KullaniciId, kdp.DiziId }) ile composite primary key tanımlanmalıdır.
    public class KullaniciDiziPuan
    {
        public int KullaniciId { get; set; }
        public Kullanici Kullanici { get; set; } = null!;

        public int DiziId { get; set; }
        public Dizi Dizi { get; set; } = null!;

        [Required]
        [Range(1, 5)] // Puan 1 ile 5 arasında olmalı
        public int Puan { get; set; }
    }
}