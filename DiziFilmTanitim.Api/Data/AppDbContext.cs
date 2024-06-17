using Microsoft.EntityFrameworkCore;
using DiziFilmTanitim.Core.Entities;

namespace DiziFilmTanitim.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Film> Filmler { get; set; } = null!;
        public DbSet<Kullanici> Kullanicilar { get; set; } = null!;
        public DbSet<Bolum> Bolumler { get; set; } = null!;
        public DbSet<Dizi> Diziler { get; set; } = null!;
        public DbSet<KullaniciDiziPuan> KullaniciDiziPuanlari { get; set; } = null!;
        public DbSet<KullaniciFilmPuan> KullaniciFilmPuanlari { get; set; } = null!;
        public DbSet<KullaniciListesi> KullaniciListeleri { get; set; } = null!;
        public DbSet<Oyuncu> Oyuncular { get; set; } = null!;
        public DbSet<Sezon> Sezonlar { get; set; } = null!;
        public DbSet<Tur> Turler { get; set; } = null!;
        public DbSet<Yonetmen> Yonetmenler { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<KullaniciFilmPuan>()
                .HasKey(kfp => new { kfp.KullaniciId, kfp.FilmId });

            modelBuilder.Entity<KullaniciFilmPuan>()
                .HasOne(kfp => kfp.Kullanici)
                .WithMany(u => u.FilmPuanlari)
                .HasForeignKey(kfp => kfp.KullaniciId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<KullaniciFilmPuan>()
                .HasOne(kfp => kfp.Film)
                .WithMany(f => f.KullaniciPuanlari)
                .HasForeignKey(kfp => kfp.FilmId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<KullaniciDiziPuan>()
                .HasKey(kdp => new { kdp.KullaniciId, kdp.DiziId });

            modelBuilder.Entity<KullaniciDiziPuan>()
                .HasOne(kdp => kdp.Kullanici)
                .WithMany(u => u.DiziPuanlari)
                .HasForeignKey(kdp => kdp.KullaniciId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<KullaniciDiziPuan>()
                .HasOne(kdp => kdp.Dizi)
                .WithMany(d => d.KullaniciPuanlari)
                .HasForeignKey(kdp => kdp.DiziId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Film>()
                .HasMany(f => f.Turler)
                .WithMany(t => t.Filmler);

            modelBuilder.Entity<Dizi>()
                .HasMany(d => d.Turler)
                .WithMany(t => t.Diziler);

            modelBuilder.Entity<Film>()
                .HasMany(f => f.Oyuncular)
                .WithMany(o => o.Filmler);

            modelBuilder.Entity<Dizi>()
                .HasMany(d => d.Oyuncular)
                .WithMany(o => o.Diziler);

            modelBuilder.Entity<Film>()
                .HasMany(f => f.KullaniciListeleri)
                .WithMany(kl => kl.Filmler);

            modelBuilder.Entity<Dizi>()
                .HasMany(d => d.KullaniciListeleri)
                .WithMany(kl => kl.Diziler);
        }
    }
}