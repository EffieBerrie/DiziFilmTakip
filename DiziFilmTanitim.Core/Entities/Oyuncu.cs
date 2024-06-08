using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace DiziFilmTanitim.Core.Entities
{
    public class Oyuncu
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string AdSoyad { get; set; } = null!;

        public DateTime? DogumTarihi { get; set; }

        [MaxLength(2000)] // Biyografi için biraz daha uzun bir alan
        public string? Biyografi { get; set; }

        public string? FotografDosyaAdi { get; set; } // Oyuncu fotoğrafının dosya adı (örn: oyuncu.jpg)

        // Bir oyuncunun rol aldığı filmler (Çoktan-çoğa ilişki)
        public ICollection<Film> Filmler { get; set; } = new List<Film>();

        // Bir oyuncunun rol aldığı diziler (Çoktan-çoğa ilişki)
        public ICollection<Dizi> Diziler { get; set; } = new List<Dizi>();
    }
}