using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace DiziFilmTanitim.Core.Entities
{
    public class Sezon
    {
        public int Id { get; set; }

        [Required]
        public int SezonNumarasi { get; set; }

        [MaxLength(200)]
        public string? Ad { get; set; } // Sezona özel bir ad olabilir (örn: "The Winter")

        public DateTime? YayinTarihi { get; set; } // Sezonun yayınlandığı tarih

        // Dizi ilişkisi
        [Required]
        public int DiziId { get; set; } // Foreign key
        public Dizi Dizi { get; set; } = null!; // Navigation property

        // Bir sezona ait bölümler
        public ICollection<Bolum> Bolumler { get; set; } = new List<Bolum>();
    }
}