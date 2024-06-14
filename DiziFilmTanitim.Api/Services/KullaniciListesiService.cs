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
    public class KullaniciListesiService : IKullaniciListesiService
    {
        private readonly AppDbContext _context;

        public KullaniciListesiService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<KullaniciListesi> CreateKullaniciListesiAsync(int kullaniciId, KullaniciListesi kullaniciListesi)
        {
            kullaniciListesi.KullaniciId = kullaniciId;
            // Kontrol: Aynı kullanıcı için aynı isimde başka bir liste var mı?
            var existingList = await _context.KullaniciListeleri
                                           .FirstOrDefaultAsync(kl => kl.KullaniciId == kullaniciId && kl.ListeAdi.ToLower() == kullaniciListesi.ListeAdi.ToLower());
            if (existingList != null)
            {
                throw new InvalidOperationException($"'{kullaniciListesi.ListeAdi}' adında bir liste zaten mevcut.");
            }

            _context.KullaniciListeleri.Add(kullaniciListesi);
            await _context.SaveChangesAsync();
            return kullaniciListesi;
        }

        public async Task DeleteKullaniciListesiAsync(int listeId)
        {
            var liste = await _context.KullaniciListeleri.FindAsync(listeId);
            if (liste != null)
            {
                _context.KullaniciListeleri.Remove(liste);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<KullaniciListesi>> GetKullaniciListeleriAsync(int kullaniciId)
        {
            return await _context.KullaniciListeleri
                                 .Where(kl => kl.KullaniciId == kullaniciId)
                                 .Include(kl => kl.Filmler) // Listedeki filmleri de getirelim (sayısı çok olabilir, dikkatli kullanılmalı)
                                 .Include(kl => kl.Diziler) // Listedeki dizileri de getirelim
                                 .OrderBy(kl => kl.ListeAdi)
                                 .ToListAsync();
        }

        public async Task<KullaniciListesi?> GetKullaniciListesiByIdAsync(int listeId)
        {
            return await _context.KullaniciListeleri
                                 .Include(kl => kl.Filmler)
                                 .Include(kl => kl.Diziler)
                                 .FirstOrDefaultAsync(kl => kl.Id == listeId);
        }

        public async Task UpdateKullaniciListesiAsync(KullaniciListesi kullaniciListesi)
        {
            // Kontrol: Aynı kullanıcı için güncellenen isimle başka bir liste (mevcut liste hariç) var mı?
            var existingListWithSameName = await _context.KullaniciListeleri
                                                       .FirstOrDefaultAsync(kl => kl.KullaniciId == kullaniciListesi.KullaniciId &&
                                                                              kl.ListeAdi.ToLower() == kullaniciListesi.ListeAdi.ToLower() &&
                                                                              kl.Id != kullaniciListesi.Id);
            if (existingListWithSameName != null)
            {
                throw new InvalidOperationException($"'{kullaniciListesi.ListeAdi}' adında başka bir liste zaten mevcut.");
            }

            var existingListe = await _context.KullaniciListeleri.FindAsync(kullaniciListesi.Id);
            if (existingListe != null)
            {
                existingListe.ListeAdi = kullaniciListesi.ListeAdi;
                existingListe.Aciklama = kullaniciListesi.Aciklama;
                // KullaniciId değiştirilemez varsayıyoruz.
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> AddFilmToListesiAsync(int listeId, int filmId)
        {
            var liste = await _context.KullaniciListeleri.Include(l => l.Filmler).FirstOrDefaultAsync(l => l.Id == listeId);
            var film = await _context.Filmler.FindAsync(filmId);

            if (liste == null || film == null)
            {
                return false; // Liste veya film bulunamadı
            }

            if (liste.Filmler.Any(f => f.Id == filmId))
            {
                return true; // Film zaten listede
            }

            liste.Filmler.Add(film);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveFilmFromListesiAsync(int listeId, int filmId)
        {
            var liste = await _context.KullaniciListeleri.Include(l => l.Filmler).FirstOrDefaultAsync(l => l.Id == listeId);
            var filmToRemove = liste?.Filmler.FirstOrDefault(f => f.Id == filmId);

            if (liste == null || filmToRemove == null)
            {
                return false; // Liste bulunamadı veya film listede değil
            }

            liste.Filmler.Remove(filmToRemove);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddDiziToListesiAsync(int listeId, int diziId)
        {
            var liste = await _context.KullaniciListeleri.Include(l => l.Diziler).FirstOrDefaultAsync(l => l.Id == listeId);
            var dizi = await _context.Diziler.FindAsync(diziId);

            if (liste == null || dizi == null)
            {
                return false; // Liste veya dizi bulunamadı
            }

            if (liste.Diziler.Any(d => d.Id == diziId))
            {
                return true; // Dizi zaten listede
            }

            liste.Diziler.Add(dizi);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveDiziFromListesiAsync(int listeId, int diziId)
        {
            var liste = await _context.KullaniciListeleri.Include(l => l.Diziler).FirstOrDefaultAsync(l => l.Id == listeId);
            var diziToRemove = liste?.Diziler.FirstOrDefault(d => d.Id == diziId);

            if (liste == null || diziToRemove == null)
            {
                return false; // Liste bulunamadı veya dizi listede değil
            }

            liste.Diziler.Remove(diziToRemove);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Film>> GetFilmlerByListeIdAsync(int listeId)
        {
            var liste = await _context.KullaniciListeleri.Include(l => l.Filmler).FirstOrDefaultAsync(l => l.Id == listeId);
            return liste?.Filmler ?? new List<Film>();
        }

        public async Task<IEnumerable<Dizi>> GetDizilerByListeIdAsync(int listeId)
        {
            var liste = await _context.KullaniciListeleri.Include(l => l.Diziler).FirstOrDefaultAsync(l => l.Id == listeId);
            return liste?.Diziler ?? new List<Dizi>();
        }
    }
}