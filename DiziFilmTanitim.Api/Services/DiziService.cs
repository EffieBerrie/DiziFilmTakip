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
    public class DiziService : IDiziService
    {
        private readonly AppDbContext _context;

        public DiziService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Dizi> AddDiziAsync(Dizi dizi, List<int>? turIdleri = null, List<int>? oyuncuIdleri = null, int? yonetmenId = null)
        {
            // Yönetmen ataması
            if (yonetmenId.HasValue)
            {
                dizi.YonetmenId = yonetmenId.Value;
            }

            _context.Diziler.Add(dizi);
            await _context.SaveChangesAsync(); // Dizi ID'sinin oluşması için önce diziyi kaydediyoruz

            if (turIdleri != null && turIdleri.Any())
            {
                var turler = await _context.Turler.Where(t => turIdleri.Contains(t.Id)).ToListAsync();
                dizi.Turler = turler;
            }

            if (oyuncuIdleri != null && oyuncuIdleri.Any())
            {
                var oyuncular = await _context.Oyuncular.Where(o => oyuncuIdleri.Contains(o.Id)).ToListAsync();
                dizi.Oyuncular = oyuncular;
            }

            await _context.SaveChangesAsync(); // İlişkileri kaydet
            return dizi;
        }

        public async Task DeleteDiziAsync(int id)
        {
            var dizi = await _context.Diziler.FindAsync(id);
            if (dizi != null)
            {
                _context.Diziler.Remove(dizi);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Dizi>> GetAllDizilerAsync(string? tur = null, string? aramaKelimesi = null, int? yapimYili = null, DiziDurumu? durum = null, string? yonetmenAdi = null)
        {
            var query = _context.Diziler
                                .Include(d => d.Yonetmen)
                                .Include(d => d.Turler)
                                .Include(d => d.Oyuncular)
                                .Include(d => d.Sezonlar)
                                .AsQueryable();

            if (!string.IsNullOrEmpty(tur))
            {
                query = query.Where(d => d.Turler.Any(t => t.Ad.ToLower() == tur.ToLower()));
            }

            if (!string.IsNullOrEmpty(aramaKelimesi))
            {
                query = query.Where(d => d.Ad.ToLower().Contains(aramaKelimesi.ToLower()) || (d.Ozet != null && d.Ozet.ToLower().Contains(aramaKelimesi.ToLower())));
            }

            if (yapimYili.HasValue)
            {
                query = query.Where(d => d.YapimYili == yapimYili.Value);
            }

            if (durum.HasValue)
            {
                query = query.Where(d => d.Durum == durum.Value);
            }

            if (!string.IsNullOrEmpty(yonetmenAdi))
            {
                query = query.Where(d => d.Yonetmen != null && d.Yonetmen.AdSoyad.ToLower().Contains(yonetmenAdi.ToLower()));
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Dizi>> GetAllDizilerSimpleAsync()
        {
            return await _context.Diziler
                                .Include(d => d.Yonetmen)
                                .Include(d => d.Turler)
                                .Include(d => d.Oyuncular)
                                .Include(d => d.Sezonlar)
                                .ToListAsync();
        }

        public async Task<Dizi?> GetDiziByIdAsync(int id)
        {
            return await _context.Diziler
                                 .Include(d => d.Yonetmen)
                                 .Include(d => d.Turler)
                                 .Include(d => d.Oyuncular)
                                 .Include(d => d.Sezonlar)
                                    .ThenInclude(s => s.Bolumler)
                                 .Include(d => d.KullaniciPuanlari)
                                 .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task UpdateDiziAsync(Dizi dizi, List<int>? turIdleri = null, List<int>? oyuncuIdleri = null, int? yonetmenId = null)
        {
            var existingDizi = await _context.Diziler
                                            .Include(d => d.Turler)
                                            .Include(d => d.Oyuncular)
                                            .FirstOrDefaultAsync(d => d.Id == dizi.Id);

            if (existingDizi == null) return;

            _context.Entry(existingDizi).CurrentValues.SetValues(dizi);

            if (yonetmenId.HasValue)
            {
                existingDizi.YonetmenId = yonetmenId;
            }
            else if (dizi.YonetmenId == null && yonetmenId == null)
            {
                existingDizi.YonetmenId = null;
            }


            if (turIdleri != null)
            {
                existingDizi.Turler.Clear();
                if (turIdleri.Any())
                {
                    var turler = await _context.Turler.Where(t => turIdleri.Contains(t.Id)).ToListAsync();
                    existingDizi.Turler = turler;
                }
            }

            if (oyuncuIdleri != null)
            {
                existingDizi.Oyuncular.Clear();
                if (oyuncuIdleri.Any())
                {
                    var oyuncular = await _context.Oyuncular.Where(o => oyuncuIdleri.Contains(o.Id)).ToListAsync();
                    existingDizi.Oyuncular = oyuncular;
                }
            }

            await _context.SaveChangesAsync();
        }

        // Sezon İşlemleri
        public async Task<Sezon> AddSezonAsync(int diziId, Sezon sezon)
        {
            sezon.DiziId = diziId;
            _context.Sezonlar.Add(sezon);
            await _context.SaveChangesAsync();
            return sezon;
        }

        public async Task DeleteSezonAsync(int sezonId)
        {
            var sezon = await _context.Sezonlar.FindAsync(sezonId);
            if (sezon != null)
            {
                _context.Sezonlar.Remove(sezon);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Sezon?> GetSezonByIdAsync(int sezonId)
        {
            return await _context.Sezonlar.Include(s => s.Bolumler).FirstOrDefaultAsync(s => s.Id == sezonId);
        }

        public async Task<IEnumerable<Sezon>> GetSezonlarByDiziAsync(int diziId)
        {
            return await _context.Sezonlar.Where(s => s.DiziId == diziId).Include(s => s.Bolumler).ToListAsync();
        }

        public async Task UpdateSezonAsync(Sezon sezon)
        {
            var existingSezon = await _context.Sezonlar.FindAsync(sezon.Id);
            if (existingSezon != null)
            {
                _context.Entry(existingSezon).CurrentValues.SetValues(sezon);
                await _context.SaveChangesAsync();
            }
        }

        // Bölüm İşlemleri
        public async Task<Bolum> AddBolumAsync(int sezonId, Bolum bolum)
        {
            bolum.SezonId = sezonId;
            _context.Bolumler.Add(bolum);
            await _context.SaveChangesAsync();
            return bolum;
        }

        public async Task DeleteBolumAsync(int bolumId)
        {
            var bolum = await _context.Bolumler.FindAsync(bolumId);
            if (bolum != null)
            {
                _context.Bolumler.Remove(bolum);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Bolum?> GetBolumByIdAsync(int bolumId)
        {
            return await _context.Bolumler.FindAsync(bolumId);
        }

        public async Task<IEnumerable<Bolum>> GetBolumlerBySezonAsync(int sezonId)
        {
            return await _context.Bolumler.Where(b => b.SezonId == sezonId).ToListAsync();
        }

        public async Task UpdateBolumAsync(Bolum bolum)
        {
            var existingBolum = await _context.Bolumler.FindAsync(bolum.Id);
            if (existingBolum != null)
            {
                _context.Entry(existingBolum).CurrentValues.SetValues(bolum);
                await _context.SaveChangesAsync();
            }
        }

        // Puanlama İşlemleri
        public async Task<bool> RateDiziAsync(int kullaniciId, int diziId, int puan)
        {
            if (puan < 1 || puan > 5) return false; // Puan 1-5 arası olmalı

            var existingPuan = await _context.KullaniciDiziPuanlari
                                             .FirstOrDefaultAsync(p => p.KullaniciId == kullaniciId && p.DiziId == diziId);

            if (existingPuan != null)
            {
                existingPuan.Puan = puan;
            }
            else
            {
                _context.KullaniciDiziPuanlari.Add(new KullaniciDiziPuan { KullaniciId = kullaniciId, DiziId = diziId, Puan = puan });
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<double?> GetDiziAverageRatingAsync(int diziId)
        {
            var dizi = await _context.Diziler.Include(d => d.KullaniciPuanlari).FirstOrDefaultAsync(d => d.Id == diziId);
            if (dizi == null || !dizi.KullaniciPuanlari.Any()) return null;
            return dizi.KullaniciPuanlari.Average(p => p.Puan);
        }

        public async Task<KullaniciDiziPuan?> GetUserRatingForDiziAsync(int kullaniciId, int diziId)
        {
            return await _context.KullaniciDiziPuanlari.FirstOrDefaultAsync(p => p.KullaniciId == kullaniciId && p.DiziId == diziId);
        }

        public async Task<IEnumerable<Dizi>> GetDizilerByYonetmenAsync(int yonetmenId)
        {
            return await _context.Diziler
                                .Include(f => f.Yonetmen)
                                .Include(f => f.Turler)
                                .Include(f => f.Oyuncular)
                                .Where(f => f.YonetmenId == yonetmenId)
                                .ToListAsync();
        }

        public async Task<IEnumerable<Dizi>> GetDizilerByOyuncuAsync(int oyuncuId)
        {
            return await _context.Diziler
                               .Include(f => f.Yonetmen)
                               .Include(f => f.Turler)
                               .Include(f => f.Oyuncular)
                               .Where(f => f.Oyuncular.Any(o => o.Id == oyuncuId))
                               .ToListAsync();
        }

        public async Task<IEnumerable<Dizi>> GetDizilerByTurAsync(int turId)
        {
            return await _context.Diziler
                                .Include(d => d.Yonetmen)
                                .Include(d => d.Turler)
                                .Include(d => d.Oyuncular)
                                .Include(d => d.Sezonlar)
                                .Where(d => d.Turler.Any(t => t.Id == turId))
                                .ToListAsync();
        }

        // Upload İşlemleri
        public async Task<bool> UpdateDiziAfisAsync(int diziId, string afisDosyaAdi)
        {
            var dizi = await _context.Diziler.FindAsync(diziId);
            if (dizi == null)
                return false;

            dizi.AfisDosyaAdi = afisDosyaAdi;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}