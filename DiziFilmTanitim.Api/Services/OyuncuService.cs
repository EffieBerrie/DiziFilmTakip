using DiziFilmTanitim.Api.Data;
using DiziFilmTanitim.Core.Entities;
using DiziFilmTanitim.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiziFilmTanitim.Api.Services
{
    public class OyuncuService : IOyuncuService
    {
        private readonly AppDbContext _context;

        public OyuncuService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Oyuncu> AddOyuncuAsync(Oyuncu oyuncu)
        {
            // İsteğe bağlı: Aynı ad soyadda başka bir oyuncu var mı kontrolü eklenebilir.
            _context.Oyuncular.Add(oyuncu);
            await _context.SaveChangesAsync();
            return oyuncu;
        }

        public async Task DeleteOyuncuAsync(int id)
        {
            var oyuncu = await _context.Oyuncular.FindAsync(id);
            if (oyuncu != null)
            {
                // Kontrol: Bu oyuncu herhangi bir filmde veya dizide rol alıyor mu?
                var oyuncuWithRelations = await _context.Oyuncular
                                                    .Include(o => o.Filmler)
                                                    .Include(o => o.Diziler)
                                                    .FirstOrDefaultAsync(o => o.Id == id);

                if (oyuncuWithRelations != null && (oyuncuWithRelations.Filmler.Any() || oyuncuWithRelations.Diziler.Any()))
                {
                    throw new InvalidOperationException("Bu oyuncu filmlerde veya dizilerde rol aldığından silinemez. Önce ilişkili kayıtları güncelleyin.");
                }

                _context.Oyuncular.Remove(oyuncu);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Oyuncu>> GetAllOyuncularAsync(string? aramaKelimesi = null)
        {
            var query = _context.Oyuncular.AsQueryable();
            if (!string.IsNullOrEmpty(aramaKelimesi))
            {
                query = query.Where(o => o.AdSoyad.ToLower().Contains(aramaKelimesi.ToLower()));
            }
            return await query.OrderBy(o => o.AdSoyad).ToListAsync();
        }

        public async Task<Oyuncu?> GetOyuncuByIdAsync(int id)
        {
            return await _context.Oyuncular
                                 .Include(o => o.Filmler) // Oyuncunun filmlerini de getirelim
                                 .Include(o => o.Diziler) // Oyuncunun dizilerini de getirelim
                                 .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task UpdateOyuncuAsync(Oyuncu oyuncu)
        {
            var existingOyuncu = await _context.Oyuncular.FindAsync(oyuncu.Id);
            if (existingOyuncu != null)
            {
                _context.Entry(existingOyuncu).CurrentValues.SetValues(oyuncu);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Oyuncu>> GetOyuncularByFilmAsync(int filmId)
        {
            return await _context.Oyuncular
                                .Where(o => o.Filmler.Any(f => f.Id == filmId))
                                .ToListAsync();
        }

        public async Task<IEnumerable<Oyuncu>> GetOyuncularByDiziAsync(int diziId)
        {
            return await _context.Oyuncular
                                .Where(o => o.Diziler.Any(d => d.Id == diziId))
                                .ToListAsync();
        }

        // Upload İşlemleri
        public async Task<bool> UpdateOyuncuFotografAsync(int oyuncuId, string fotografDosyaAdi)
        {
            var oyuncu = await _context.Oyuncular.FindAsync(oyuncuId);
            if (oyuncu == null)
                return false;

            oyuncu.FotografDosyaAdi = fotografDosyaAdi;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}