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
    public class FilmService : IFilmService
    {
        private readonly AppDbContext _context;

        public FilmService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Film> AddFilmAsync(Film film, List<int>? turIdleri = null, List<int>? oyuncuIdleri = null, int? yonetmenId = null)
        {
            // Yönetmen ataması
            if (yonetmenId.HasValue)
            {
                film.YonetmenId = yonetmenId.Value;
            }

            _context.Filmler.Add(film);
            await _context.SaveChangesAsync(); // Film ID'sinin oluşması için önce filmi kaydediyoruz

            // Tür ilişkilerini ekle
            if (turIdleri != null && turIdleri.Any())
            {
                var turler = await _context.Turler.Where(t => turIdleri.Contains(t.Id)).ToListAsync();
                film.Turler = turler;
            }

            // Oyuncu ilişkilerini ekle
            if (oyuncuIdleri != null && oyuncuIdleri.Any())
            {
                var oyuncular = await _context.Oyuncular.Where(o => oyuncuIdleri.Contains(o.Id)).ToListAsync();
                film.Oyuncular = oyuncular;
            }

            await _context.SaveChangesAsync(); // İlişkileri kaydet
            return film;
        }

        public async Task DeleteFilmAsync(int id)
        {
            var film = await _context.Filmler.FindAsync(id);
            if (film != null)
            {
                _context.Filmler.Remove(film);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Film>> GetAllFilmlerAsync(string? tur = null, string? aramaKelimesi = null, int? yapimYili = null, string? yonetmenAdi = null)
        {
            var query = _context.Filmler
                                .Include(f => f.Yonetmen)
                                .Include(f => f.Turler)
                                .Include(f => f.Oyuncular)
                                .AsQueryable();

            if (!string.IsNullOrEmpty(tur))
            {
                query = query.Where(f => f.Turler.Any(t => t.Ad.ToLower() == tur.ToLower()));
            }

            if (!string.IsNullOrEmpty(aramaKelimesi))
            {
                query = query.Where(f => f.Ad.ToLower().Contains(aramaKelimesi.ToLower()) || (f.Ozet != null && f.Ozet.ToLower().Contains(aramaKelimesi.ToLower())));
            }

            if (yapimYili.HasValue)
            {
                query = query.Where(f => f.YapimYili == yapimYili.Value);
            }

            if (!string.IsNullOrEmpty(yonetmenAdi))
            {
                query = query.Where(f => f.Yonetmen != null && f.Yonetmen.AdSoyad.ToLower().Contains(yonetmenAdi.ToLower()));
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Film>> GetAllFilmlerSimpleAsync()
        {
            return await _context.Filmler
                                .Include(f => f.Yonetmen)
                                .Include(f => f.Turler)
                                .Include(f => f.Oyuncular)
                                .ToListAsync();
        }

        public async Task<Film?> GetFilmByIdAsync(int id)
        {
            return await _context.Filmler
                                 .Include(f => f.Yonetmen)
                                 .Include(f => f.Turler)
                                 .Include(f => f.Oyuncular)
                                 .Include(f => f.KullaniciPuanlari)
                                 .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task UpdateFilmAsync(Film film, List<int>? turIdleri = null, List<int>? oyuncuIdleri = null, int? yonetmenId = null)
        {
            // Önce mevcut filmi ve ilişkilerini yükleyelim
            var existingFilm = await _context.Filmler
                                            .Include(f => f.Turler)
                                            .Include(f => f.Oyuncular)
                                            .FirstOrDefaultAsync(f => f.Id == film.Id);

            if (existingFilm == null)
            {
                // Film bulunamadı, isteğe bağlı olarak hata fırlatılabilir veya işlem yapılmayabilir.
                return;
            }

            // Temel film bilgilerini güncelle
            _context.Entry(existingFilm).CurrentValues.SetValues(film);

            // Yönetmen güncellemesi (varsa)
            if (yonetmenId.HasValue)
            {
                existingFilm.YonetmenId = yonetmenId;
            }
            else if (film.YonetmenId == null && yonetmenId == null)
            {
                existingFilm.YonetmenId = null;
            }

            // Tür ilişkilerini güncelle (varsa)
            if (turIdleri != null)
            {
                existingFilm.Turler.Clear();
                if (turIdleri.Any())
                {
                    var turler = await _context.Turler.Where(t => turIdleri.Contains(t.Id)).ToListAsync();
                    existingFilm.Turler = turler;
                }
            }

            // Oyuncu ilişkilerini güncelle (varsa)
            if (oyuncuIdleri != null)
            {
                existingFilm.Oyuncular.Clear();
                if (oyuncuIdleri.Any())
                {
                    var oyuncular = await _context.Oyuncular.Where(o => oyuncuIdleri.Contains(o.Id)).ToListAsync();
                    existingFilm.Oyuncular = oyuncular;
                }
            }

            // Değişiklikleri kaydet
            await _context.SaveChangesAsync();
        }


        // Puanlama ve Liste İşlemleri
        public async Task<bool> RateFilmAsync(int kullaniciId, int filmId, int puan)
        {
            if (puan < 1 || puan > 5) // Entity'deki [Range(1,5)] ile uyumlu
            {
                return false; // Puan aralığı geçersiz
            }

            var existingPuan = await _context.KullaniciFilmPuanlari
                                             .FirstOrDefaultAsync(p => p.KullaniciId == kullaniciId && p.FilmId == filmId);

            if (existingPuan != null)
            {
                existingPuan.Puan = puan;
            }
            else
            {
                var yeniPuan = new KullaniciFilmPuan
                {
                    KullaniciId = kullaniciId,
                    FilmId = filmId,
                    Puan = puan
                };
                _context.KullaniciFilmPuanlari.Add(yeniPuan);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<double?> GetFilmAverageRatingAsync(int filmId)
        {
            var film = await _context.Filmler.Include(f => f.KullaniciPuanlari).FirstOrDefaultAsync(f => f.Id == filmId);
            if (film == null || !film.KullaniciPuanlari.Any())
            {
                return null;
            }
            return film.KullaniciPuanlari.Average(p => p.Puan);
        }

        public async Task<KullaniciFilmPuan?> GetUserRatingForFilmAsync(int kullaniciId, int filmId)
        {
            return await _context.KullaniciFilmPuanlari
                                 .FirstOrDefaultAsync(p => p.KullaniciId == kullaniciId && p.FilmId == filmId);
        }

        public async Task<IEnumerable<Film>> GetFilmlerByYonetmenAsync(int yonetmenId)
        {
            return await _context.Filmler
                                .Include(f => f.Yonetmen)
                                .Include(f => f.Turler)
                                .Include(f => f.Oyuncular)
                                .Where(f => f.YonetmenId == yonetmenId)
                                .ToListAsync();
        }

        public async Task<IEnumerable<Film>> GetFilmlerByOyuncuAsync(int oyuncuId)
        {
            return await _context.Filmler
                               .Include(f => f.Yonetmen)
                               .Include(f => f.Turler)
                               .Include(f => f.Oyuncular)
                               .Where(f => f.Oyuncular.Any(o => o.Id == oyuncuId))
                               .ToListAsync();
        }

        public async Task<IEnumerable<Film>> GetFilmlerByTurAsync(int turId)
        {
            return await _context.Filmler
                                .Include(f => f.Yonetmen)
                                .Include(f => f.Turler)
                                .Include(f => f.Oyuncular)
                                .Where(f => f.Turler.Any(t => t.Id == turId))
                                .ToListAsync();
        }

        // Upload İşlemleri
        public async Task<bool> UpdateFilmAfisAsync(int filmId, string afisDosyaAdi)
        {
            var film = await _context.Filmler.FindAsync(filmId);
            if (film == null)
                return false;

            film.AfisDosyaAdi = afisDosyaAdi;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}