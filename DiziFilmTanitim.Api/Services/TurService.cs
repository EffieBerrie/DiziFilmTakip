using DiziFilmTanitim.Api.Data;
using DiziFilmTanitim.Core.Entities;
using DiziFilmTanitim.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace DiziFilmTanitim.Api.Services
{
    public class TurService : ITurService
    {
        private readonly AppDbContext _context;

        public TurService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Tur> AddTurAsync(Tur tur)
        {
            // Kontrol: Aynı isimde başka bir tür var mı?
            var existingTur = await _context.Turler.FirstOrDefaultAsync(t => t.Ad.ToLower() == tur.Ad.ToLower());
            if (existingTur != null)
            {
                throw new InvalidOperationException($"'{tur.Ad}' adında bir tür zaten mevcut.");
            }

            _context.Turler.Add(tur);
            await _context.SaveChangesAsync();
            return tur;
        }

        public async Task DeleteTurAsync(int id)
        {
            var tur = await _context.Turler.FindAsync(id);
            if (tur != null)
            {
                // Kontrol: Bu türe bağlı film veya dizi var mı?
                bool hasFilms = await _context.Filmler.AnyAsync(f => f.Turler.Any(t => t.Id == id));
                bool hasDizis = await _context.Diziler.AnyAsync(d => d.Turler.Any(t => t.Id == id));

                if (hasFilms || hasDizis)
                {
                    throw new InvalidOperationException("Bu türe bağlı filmler veya diziler olduğundan silinemez. Önce ilişkili kayıtları güncelleyin.");
                }

                _context.Turler.Remove(tur);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Tur>> GetAllTurlerAsync(string? aramaKelimesi = null)
        {
            var query = _context.Turler.AsQueryable();

            if (!string.IsNullOrEmpty(aramaKelimesi))
            {
                query = query.Where(t => t.Ad.ToLower().Contains(aramaKelimesi.ToLower()));
            }

            return await query.OrderBy(t => t.Ad).ToListAsync();
        }

        public async Task<Tur?> GetTurByIdAsync(int id)
        {
            return await _context.Turler.FindAsync(id);
            // İlişkili filmleri ve dizileri getirmek istersek:
            // return await _context.Turler
            //                      .Include(t => t.Filmler)
            //                      .Include(t => t.Diziler)
            //                      .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task UpdateTurAsync(Tur tur)
        {
            // Kontrol: Güncellenmek istenen Ad ile başka bir tür (mevcut tür hariç) var mı?
            var existingTurWithSameName = await _context.Turler
                                                .FirstOrDefaultAsync(t => t.Ad.ToLower() == tur.Ad.ToLower() && t.Id != tur.Id);
            if (existingTurWithSameName != null)
            {
                throw new InvalidOperationException($"'{tur.Ad}' adında başka bir tür zaten mevcut.");
            }

            var existingTur = await _context.Turler.FindAsync(tur.Id);
            if (existingTur != null)
            {
                _context.Entry(existingTur).CurrentValues.SetValues(tur);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new ArgumentException($"ID {tur.Id} ile bir tür bulunamadı.");
            }
        }
    }
}