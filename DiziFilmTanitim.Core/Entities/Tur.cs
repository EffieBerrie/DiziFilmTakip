using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace DiziFilmTanitim.Core.Entities
{
    public class Tur
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Ad { get; set; } = null!;

        // Bir türe ait filmler
        public ICollection<Film> Filmler { get; set; } = new List<Film>();

        // Bir türe ait diziler
        public ICollection<Dizi> Diziler { get; set; } = new List<Dizi>();
    }
}