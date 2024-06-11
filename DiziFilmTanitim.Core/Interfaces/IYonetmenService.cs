using DiziFilmTanitim.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiziFilmTanitim.Core.Interfaces
{
    public interface IYonetmenService
    {
        Task<IEnumerable<Yonetmen>> GetAllYonetmenlerAsync(string? aramaKelimesi = null);
        Task<Yonetmen?> GetYonetmenByIdAsync(int id);
        Task<Yonetmen> AddYonetmenAsync(Yonetmen yonetmen);
        Task UpdateYonetmenAsync(Yonetmen yonetmen);
        Task DeleteYonetmenAsync(int id);
        // Bir yönetmenin filmlerini/dizilerini getirme işlemleri FilmService ve DiziService içinde zaten mevcut.

        // Upload İşlemleri
        Task<bool> UpdateYonetmenFotografAsync(int yonetmenId, string fotografDosyaAdi);
    }
}