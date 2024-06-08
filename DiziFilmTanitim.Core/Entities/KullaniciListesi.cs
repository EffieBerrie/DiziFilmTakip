using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace DiziFilmTanitim.Core.Entities
{
    public class KullaniciListesi
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string ListeAdi { get; set; } = null!;

        [MaxLength(500)]
        public string? Aciklama { get; set; }

        // Liste hangi kullanıcıya ait?
        public int KullaniciId { get; set; }
        public Kullanici Kullanici { get; set; } = null!;

        // Bu listedeki filmler (Çoktan-çoğa ilişki)
        public ICollection<Film> Filmler { get; set; } = new List<Film>();

        // Bu listedeki diziler (Çoktan-çoğa ilişki)
        public ICollection<Dizi> Diziler { get; set; } = new List<Dizi>();
    }
}