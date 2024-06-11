using DiziFilmTanitim.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiziFilmTanitim.Core.Interfaces
{
    public interface IKullaniciService
    {
        Task<Kullanici?> LoginAsync(string kullaniciAdi, string sifre);
        Task<bool> LogoutAsync(int kullaniciId);
        Task<Kullanici> RegisterAsync(Kullanici kullanici);
        Task<Kullanici?> GetKullaniciByIdAsync(int id);
        Task<Kullanici?> GetKullaniciByKullaniciAdiAsync(string kullaniciAdi);
        Task<IEnumerable<Kullanici>> GetAllKullanicilarAsync();
        Task<bool> UpdateKullaniciAsync(Kullanici kullanici);
        Task<bool> ChangePasswordAsync(int kullaniciId, string eskiSifre, string yeniSifre);
        Task<bool> DeleteKullaniciAsync(int kullaniciId);
    }
}