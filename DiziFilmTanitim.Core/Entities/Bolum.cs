using System;
using System.ComponentModel.DataAnnotations;

namespace DiziFilmTanitim.Core.Entities
{
    public class Bolum
    {
        public int Id { get; set; }

        [Required]
        public int BolumNumarasi { get; set; } // Sezon içindeki bölüm sırası (örn: 1, 2, ...)

        [MaxLength(250)]
        public string? Ad { get; set; } // Bölüm adı

        [MaxLength(2000)] // Bölüm özeti için daha uzun bir alan
        public string? Ozet { get; set; }

        public DateTime? YayinTarihi { get; set; }

        public int? SureDakika { get; set; } // Bölüm süresi dakika cinsinden

        // Sezon ilişkisi
        [Required]
        public int SezonId { get; set; } // Foreign key
        public Sezon Sezon { get; set; } = null!; // Navigation property
    }
}