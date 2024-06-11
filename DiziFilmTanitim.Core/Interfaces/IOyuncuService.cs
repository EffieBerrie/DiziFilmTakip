using DiziFilmTanitim.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiziFilmTanitim.Core.Interfaces
{
    public interface IOyuncuService
    {
        Task<IEnumerable<Oyuncu>> GetAllOyuncularAsync(string? aramaKelimesi = null);
        Task<Oyuncu?> GetOyuncuByIdAsync(int id);
        Task<Oyuncu> AddOyuncuAsync(Oyuncu oyuncu);
        Task UpdateOyuncuAsync(Oyuncu oyuncu);
        Task DeleteOyuncuAsync(int id);
        Task<IEnumerable<Oyuncu>> GetOyuncularByFilmAsync(int filmId);
        Task<IEnumerable<Oyuncu>> GetOyuncularByDiziAsync(int diziId);

        // Upload İşlemleri
        Task<bool> UpdateOyuncuFotografAsync(int oyuncuId, string fotografDosyaAdi);
        // Bir oyuncunun filmlerini/dizilerini getirme işlemleri FilmService ve DiziService içinde zaten mevcut.
    }
}