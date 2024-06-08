using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace DiziFilmTanitim.Core.Entities
{
    public class Yonetmen
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string AdSoyad { get; set; } = null!;

        public DateTime? DogumTarihi { get; set; }

        [MaxLength(2000)]
        public string? Biyografi { get; set; }

        public string? FotografDosyaAdi { get; set; }

        public ICollection<Film> Filmler { get; set; } = new List<Film>();
        public ICollection<Dizi> Diziler { get; set; } = new List<Dizi>();
    }
}