using DiziFilmTanitim.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiziFilmTanitim.Core.Interfaces
{
    public interface IKullaniciListesiService
    {
        Task<IEnumerable<KullaniciListesi>> GetKullaniciListeleriAsync(int kullaniciId);
        Task<KullaniciListesi?> GetKullaniciListesiByIdAsync(int listeId);
        Task<KullaniciListesi> CreateKullaniciListesiAsync(int kullaniciId, KullaniciListesi kullaniciListesi);
        Task UpdateKullaniciListesiAsync(KullaniciListesi kullaniciListesi);
        Task DeleteKullaniciListesiAsync(int listeId);

        Task<bool> AddFilmToListesiAsync(int listeId, int filmId);
        Task<bool> RemoveFilmFromListesiAsync(int listeId, int filmId);
        Task<bool> AddDiziToListesiAsync(int listeId, int diziId);
        Task<bool> RemoveDiziFromListesiAsync(int listeId, int diziId);

        Task<IEnumerable<Film>> GetFilmlerByListeIdAsync(int listeId);
        Task<IEnumerable<Dizi>> GetDizilerByListeIdAsync(int listeId);
    }
}