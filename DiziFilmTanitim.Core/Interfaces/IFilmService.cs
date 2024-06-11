using DiziFilmTanitim.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiziFilmTanitim.Core.Interfaces
{
    public interface IFilmService
    {
        Task<IEnumerable<Film>> GetAllFilmlerAsync(string? tur = null, string? aramaKelimesi = null, int? yapimYili = null, string? yonetmenAdi = null);
        Task<IEnumerable<Film>> GetAllFilmlerSimpleAsync(); // Tüm filmleri getir (basit versiyon)
        Task<Film?> GetFilmByIdAsync(int id);
        Task<Film> AddFilmAsync(Film film, List<int>? turIdleri = null, List<int>? oyuncuIdleri = null, int? yonetmenId = null);
        Task UpdateFilmAsync(Film film, List<int>? turIdleri = null, List<int>? oyuncuIdleri = null, int? yonetmenId = null);
        Task DeleteFilmAsync(int id);

        // Puanlama ve Liste İşlemleri
        Task<bool> RateFilmAsync(int kullaniciId, int filmId, int puan); // Puan 1-5 arası
        Task<double?> GetFilmAverageRatingAsync(int filmId);
        Task<KullaniciFilmPuan?> GetUserRatingForFilmAsync(int kullaniciId, int filmId);

        Task<IEnumerable<Film>> GetFilmlerByYonetmenAsync(int yonetmenId);
        Task<IEnumerable<Film>> GetFilmlerByOyuncuAsync(int oyuncuId);
        Task<IEnumerable<Film>> GetFilmlerByTurAsync(int turId);

        // Upload İşlemleri
        Task<bool> UpdateFilmAfisAsync(int filmId, string afisDosyaAdi);
    }
}