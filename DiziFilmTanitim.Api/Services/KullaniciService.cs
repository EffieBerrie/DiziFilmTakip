using DiziFilmTanitim.Api.Data;
using DiziFilmTanitim.Core.Entities;
using DiziFilmTanitim.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiziFilmTanitim.Api.Services
{
    public class KullaniciService : IKullaniciService
    {
        private readonly AppDbContext _context;

        public KullaniciService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Kullanici>> GetAllKullanicilarAsync()
        {
            return await _context.Kullanicilar
                                 .Include(k => k.KullaniciListeleri)
                                 .Include(k => k.FilmPuanlari)
                                 .Include(k => k.DiziPuanlari)
                                 .OrderBy(k => k.KullaniciAdi)
                                 .ToListAsync();
        }

        public async Task<Kullanici?> LoginAsync(string kullaniciAdi, string sifre)
        {

            return await _context.Kullanicilar
                                 .FirstOrDefaultAsync(k => k.KullaniciAdi == kullaniciAdi && k.Sifre == sifre);
        }

        public async Task<bool> LogoutAsync(int kullaniciId)
        {

            var kullanici = await _context.Kullanicilar.FindAsync(kullaniciId);
            if (kullanici == null)
            {
                throw new ArgumentException($"ID {kullaniciId} ile bir kullanıcı bulunamadı.");
            }

           
            return true;
        }

        public async Task<Kullanici> RegisterAsync(Kullanici kullanici)
        {
            // Kullanıcı adı benzersiz olmalı
            var existingUser = await _context.Kullanicilar.FirstOrDefaultAsync(k => k.KullaniciAdi == kullanici.KullaniciAdi);
            if (existingUser != null)
            {
                throw new InvalidOperationException($"'{kullanici.KullaniciAdi}' kullanıcı adı zaten mevcut.");
            }

            // Email benzersiz olmalı (isteğe bağlı, email null olabilir)
            if (!string.IsNullOrEmpty(kullanici.Email))
            {
                var existingEmail = await _context.Kullanicilar.FirstOrDefaultAsync(k => k.Email == kullanici.Email);
                if (existingEmail != null)
                {
                    throw new InvalidOperationException($"'{kullanici.Email}' e-posta adresi zaten kullanılıyor.");
                }
            }

            _context.Kullanicilar.Add(kullanici);
            await _context.SaveChangesAsync();
            return kullanici;
        }

        public async Task<Kullanici?> GetKullaniciByIdAsync(int id)
        {
            return await _context.Kullanicilar
                                 .Include(k => k.KullaniciListeleri)
                                 .Include(k => k.FilmPuanlari)
                                 .Include(k => k.DiziPuanlari)
                                 .FirstOrDefaultAsync(k => k.Id == id);
        }

        public async Task<Kullanici?> GetKullaniciByKullaniciAdiAsync(string kullaniciAdi)
        {
            return await _context.Kullanicilar
                                 .Include(k => k.KullaniciListeleri)
                                 .Include(k => k.FilmPuanlari)
                                 .Include(k => k.DiziPuanlari)
                                 .FirstOrDefaultAsync(k => k.KullaniciAdi == kullaniciAdi);
        }

        public async Task<bool> UpdateKullaniciAsync(Kullanici kullanici)
        {
            // Sadece belirli alanların güncellenmesine izin verilebilir, örn: Email.
            // Şifre değiştirme için ayrı bir metot (ChangePasswordAsync) var.
            var existingKullanici = await _context.Kullanicilar.FindAsync(kullanici.Id);
            if (existingKullanici == null)
            {
                throw new ArgumentException($"ID {kullanici.Id} ile bir kullanıcı bulunamadı.");
            }

            if (!string.IsNullOrEmpty(kullanici.Email) && existingKullanici.Email != kullanici.Email)
            {
                var emailExists = await _context.Kullanicilar.AnyAsync(k => k.Email == kullanici.Email && k.Id != kullanici.Id);
                if (emailExists)
                {
                    throw new InvalidOperationException($"'{kullanici.Email}' e-posta adresi zaten başka bir kullanıcı tarafından kullanılıyor.");
                }
                existingKullanici.Email = kullanici.Email;
            }
            else if (string.IsNullOrEmpty(kullanici.Email) && !string.IsNullOrEmpty(existingKullanici.Email))
            {
                existingKullanici.Email = null; // Email silinmek isteniyorsa
            }
            // Kullanıcı adı değiştirilemez varsayıyoruz.
            // Diğer güncellenebilir alanlar buraya eklenebilir.

            await _context.SaveChangesAsync();
            return true; // İşlem başarılı
        }

        public async Task<bool> ChangePasswordAsync(int kullaniciId, string eskiSifre, string yeniSifre)
        {
            var kullanici = await _context.Kullanicilar.FindAsync(kullaniciId);
            if (kullanici == null)
            {
                throw new ArgumentException($"ID {kullaniciId} ile bir kullanıcı bulunamadı.");
            }

            // Şifre hashleme ve güvenlik konusuna girilmeyecek, kullanıcının isteği üzerine.
            // Mevcut düz metin karşılaştırması ve atama kalacak.
            if (kullanici.Sifre != eskiSifre)
            {
                throw new InvalidOperationException("Eski şifre yanlış.");
            }

            kullanici.Sifre = yeniSifre;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteKullaniciAsync(int kullaniciId)
        {
            var kullanici = await _context.Kullanicilar.FindAsync(kullaniciId);
            if (kullanici == null)
            {
                throw new ArgumentException($"ID {kullaniciId} ile bir kullanıcı bulunamadı.");
            }

            _context.Kullanicilar.Remove(kullanici);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}