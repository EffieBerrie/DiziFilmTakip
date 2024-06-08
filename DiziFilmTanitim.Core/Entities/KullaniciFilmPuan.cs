using System.ComponentModel.DataAnnotations;

namespace DiziFilmTanitim.Core.Entities
{
    // Kullanıcının bir filme verdiği puanı temsil eder.
    // Bu entity için AppDbContext içerisinde OnModelCreating altında
    // HasKey(kfp => new { kfp.KullaniciId, kfp.FilmId }) ile composite primary key tanımlanmalıdır.
    public class KullaniciFilmPuan
    {
        public int KullaniciId { get; set; }
        public Kullanici Kullanici { get; set; } = null!;

        public int FilmId { get; set; }
        public Film Film { get; set; } = null!;

        [Required]
        [Range(1, 5)] // Puan 1 ile 5 arasında olmalı
        public int Puan { get; set; }
    }
}