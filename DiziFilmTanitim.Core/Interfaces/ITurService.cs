using DiziFilmTanitim.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiziFilmTanitim.Core.Interfaces
{
    public interface ITurService
    {
        Task<IEnumerable<Tur>> GetAllTurlerAsync(string? aramaKelimesi = null);
        Task<Tur?> GetTurByIdAsync(int id);
        Task<Tur> AddTurAsync(Tur tur);
        Task UpdateTurAsync(Tur tur);
        Task DeleteTurAsync(int id);
    }
}