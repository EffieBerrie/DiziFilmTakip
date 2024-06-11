using DiziFilmTanitim.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiziFilmTanitim.Core.Interfaces
{
    public interface IDiziService
    {
        Task<IEnumerable<Dizi>> GetAllDizilerAsync(string? tur = null, string? aramaKelimesi = null, int? yapimYili = null, DiziDurumu? durum = null, string? yonetmenAdi = null);
        Task<IEnumerable<Dizi>> GetAllDizilerSimpleAsync();
        Task<Dizi?> GetDiziByIdAsync(int id);
        Task<Dizi> AddDiziAsync(Dizi dizi, List<int>? turIdleri = null, List<int>? oyuncuIdleri = null, int? yonetmenId = null);
        Task UpdateDiziAsync(Dizi dizi, List<int>? turIdleri = null, List<int>? oyuncuIdleri = null, int? yonetmenId = null);
        Task DeleteDiziAsync(int id);

        // Sezon İşlemleri
        Task<IEnumerable<Sezon>> GetSezonlarByDiziAsync(int diziId);
        Task<Sezon?> GetSezonByIdAsync(int sezonId);
        Task<Sezon> AddSezonAsync(int diziId, Sezon sezon);
        Task UpdateSezonAsync(Sezon sezon);
        Task DeleteSezonAsync(int sezonId);

        // Bölüm İşlemleri
        Task<IEnumerable<Bolum>> GetBolumlerBySezonAsync(int sezonId);
        Task<Bolum?> GetBolumByIdAsync(int bolumId);
        Task<Bolum> AddBolumAsync(int sezonId, Bolum bolum);
        Task UpdateBolumAsync(Bolum bolum);
        Task DeleteBolumAsync(int bolumId);

        // Puanlama İşlemleri (1-5 arası)
        Task<bool> RateDiziAsync(int kullaniciId, int diziId, int puan);
        Task<double?> GetDiziAverageRatingAsync(int diziId);
        Task<KullaniciDiziPuan?> GetUserRatingForDiziAsync(int kullaniciId, int diziId);

        Task<IEnumerable<Dizi>> GetDizilerByYonetmenAsync(int yonetmenId);
        Task<IEnumerable<Dizi>> GetDizilerByOyuncuAsync(int oyuncuId);
        Task<IEnumerable<Dizi>> GetDizilerByTurAsync(int turId);

        // Upload İşlemleri
        Task<bool> UpdateDiziAfisAsync(int diziId, string afisDosyaAdi);
    }
}