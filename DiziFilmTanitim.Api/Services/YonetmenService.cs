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
    public class YonetmenService : IYonetmenService
    {
        private readonly AppDbContext _context;

        public YonetmenService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Yonetmen> AddYonetmenAsync(Yonetmen yonetmen)
        {
            // İsteğe bağlı: Aynı ad soyadda başka bir yönetmen var mı kontrolü eklenebilir.
            _context.Yonetmenler.Add(yonetmen);
            await _context.SaveChangesAsync();
            return yonetmen;
        }

        public async Task DeleteYonetmenAsync(int id)
        {
            var yonetmen = await _context.Yonetmenler.FindAsync(id);
            if (yonetmen != null)
            {
                // Kontrol: Bu yönetmene bağlı film veya dizi var mı?
                bool hasFilms = await _context.Filmler.AnyAsync(f => f.YonetmenId == id);
                bool hasDizis = await _context.Diziler.AnyAsync(d => d.YonetmenId == id);

                if (hasFilms || hasDizis)
                {
                    throw new InvalidOperationException("Bu yönetmene atanmış filmler veya diziler olduğundan silinemez. Önce ilişkili kayıtları güncelleyin.");
                }

                _context.Yonetmenler.Remove(yonetmen);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Yonetmen>> GetAllYonetmenlerAsync(string? aramaKelimesi = null)
        {
            var query = _context.Yonetmenler.AsQueryable();
            if (!string.IsNullOrEmpty(aramaKelimesi))
            {
                query = query.Where(y => y.AdSoyad.ToLower().Contains(aramaKelimesi.ToLower()));
            }
            return await query.OrderBy(y => y.AdSoyad).ToListAsync();
        }

        public async Task<Yonetmen?> GetYonetmenByIdAsync(int id)
        {
            return await _context.Yonetmenler
                                 .Include(y => y.Filmler)  // Yönetmenin filmlerini de getirelim
                                 .Include(y => y.Diziler)  // Yönetmenin dizilerini de getirelim
                                 .FirstOrDefaultAsync(y => y.Id == id);
        }

        public async Task UpdateYonetmenAsync(Yonetmen yonetmen)
        {
            var existingYonetmen = await _context.Yonetmenler.FindAsync(yonetmen.Id);
            if (existingYonetmen != null)
            {
                _context.Entry(existingYonetmen).CurrentValues.SetValues(yonetmen);
                await _context.SaveChangesAsync();
            }
        }

        // Upload İşlemleri
        public async Task<bool> UpdateYonetmenFotografAsync(int yonetmenId, string fotografDosyaAdi)
        {
            var yonetmen = await _context.Yonetmenler.FindAsync(yonetmenId);
            if (yonetmen == null)
                return false;

            yonetmen.FotografDosyaAdi = fotografDosyaAdi;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}