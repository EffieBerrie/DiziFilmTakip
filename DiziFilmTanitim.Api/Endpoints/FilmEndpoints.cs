using DiziFilmTanitim.Core.Entities;
using DiziFilmTanitim.Core.Interfaces;
using DiziFilmTanitim.Api.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace DiziFilmTanitim.Api.Endpoints
{
    public record FilmEkleModel(
        string Ad,
        int? YapimYili,
        string? Ozet,
        string? AfisDosyaAdi,
        int? SureDakika,
        int? YonetmenId,
        List<int>? TurIdleri,
        List<int>? OyuncuIdleri
    );

    public record FilmGuncelleModel(
        int Id,
        string Ad,
        int? YapimYili,
        string? Ozet,
        string? AfisDosyaAdi,
        int? SureDakika,
        int? YonetmenId,
        List<int>? TurIdleri,
        List<int>? OyuncuIdleri
    );

    public record FilmPuanlaModel(int Puan);

    // Film-specific Response Models
    public record FilmResponseModel(
        int Id,
        string Ad,
        int? YapimYili,
        string? Ozet,
        string? AfisDosyaAdi,
        int? SureDakika,
        SimpleYonetmenResponseModel? Yonetmen,
        List<SimpleTurResponseModel> Turler,
        List<SimpleOyuncuResponseModel> Oyuncular
    );

    public static class FilmEndpoints
    {
        private static FilmResponseModel ToResponseModel(Film? film)
        {
            if (film == null) throw new ArgumentNullException(nameof(film));

            return new FilmResponseModel(
                film.Id,
                film.Ad,
                film.YapimYili,
                film.Ozet,
                film.AfisDosyaAdi,
                film.SureDakika,
                film.Yonetmen != null ? new SimpleYonetmenResponseModel(film.Yonetmen.Id, film.Yonetmen.AdSoyad) : null,
                film.Turler?.Select(t => new SimpleTurResponseModel(t.Id, t.Ad)).ToList() ?? new List<SimpleTurResponseModel>(),
                film.Oyuncular?.Select(o => new SimpleOyuncuResponseModel(o.Id, o.AdSoyad)).ToList() ?? new List<SimpleOyuncuResponseModel>()
            );
        }

        public static void MapFilmEndpoints(this IEndpointRouteBuilder app)
        {
            var grup = app.MapGroup("/api/filmler").WithTags("Film İşlemleri");

            // GET /api/filmler - Tüm filmleri listele veya ara
            grup.MapGet("/", async (IFilmService filmService, string? arama = null) =>
            {
                var filmler = await filmService.GetAllFilmlerSimpleAsync();

                // Arama varsa filtrele
                if (!string.IsNullOrWhiteSpace(arama))
                {
                    filmler = filmler.Where(f => f.Ad.Contains(arama, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                var response = new { Success = true, Data = filmler.Select(ToResponseModel).ToList() };
                return Results.Ok(response);
            });

            // GET /api/filmler/ara - Filmleri çeşitli kriterlere göre ara
            grup.MapGet("/ara", async (IFilmService filmService,
                string? ad = null,
                int? yapimYili = null,
                int? turId = null,
                int? yonetmenId = null,
                int? oyuncuId = null) =>
            {
                try
                {
                    var tumFilmler = await filmService.GetAllFilmlerSimpleAsync();
                    var query = tumFilmler.AsQueryable();

                    // Filtreleme kriterleri
                    if (!string.IsNullOrWhiteSpace(ad))
                        query = query.Where(f => f.Ad.Contains(ad, StringComparison.OrdinalIgnoreCase));

                    if (yapimYili.HasValue)
                        query = query.Where(f => f.YapimYili == yapimYili.Value);

                    if (turId.HasValue)
                        query = query.Where(f => f.Turler.Any(t => t.Id == turId.Value));

                    if (yonetmenId.HasValue)
                        query = query.Where(f => f.YonetmenId == yonetmenId.Value);

                    if (oyuncuId.HasValue)
                        query = query.Where(f => f.Oyuncular.Any(o => o.Id == oyuncuId.Value));

                    var filtrelenmisFilmler = query.ToList();
                    var response = filtrelenmisFilmler.Select(ToResponseModel);
                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Filmler aranırken hata oluştu: {ex.Message}");
                }
            });

            // GET /api/filmler/{id} - ID ile film getir
            grup.MapGet("/{id:int}", async (int id, IFilmService filmService) =>
            {
                var film = await filmService.GetFilmByIdAsync(id);
                if (film == null) return Results.NotFound(new CommonApiErrorResponseModel("Film bulunamadı."));

                var response = ToResponseModel(film);
                return Results.Ok(response);
            });

            // POST /api/filmler - Yeni film ekle
            grup.MapPost("/", async (FilmEkleModel model, IFilmService filmService) =>
            {
                try
                {
                    var yeniFilm = new Film
                    {
                        Ad = model.Ad,
                        YapimYili = model.YapimYili,
                        Ozet = model.Ozet,
                        AfisDosyaAdi = model.AfisDosyaAdi,
                        SureDakika = model.SureDakika,
                        YonetmenId = model.YonetmenId
                    };
                    var olusturulanFilm = await filmService.AddFilmAsync(yeniFilm, model.TurIdleri, model.OyuncuIdleri, model.YonetmenId);
                    var response = ToResponseModel(olusturulanFilm);
                    return Results.Created($"/api/filmler/{olusturulanFilm.Id}", response);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new CommonApiErrorResponseModel($"Film eklenirken hata oluştu: {ex.Message}"));
                }
            });

            // PUT /api/filmler/{id} - Film güncelle
            grup.MapPut("/{id:int}", async (int id, FilmGuncelleModel model, IFilmService filmService) =>
            {
                if (id != model.Id) return Results.BadRequest(new CommonApiErrorResponseModel("ID uyuşmazlığı."));

                try
                {
                    var guncellenecekFilm = new Film
                    {
                        Id = model.Id,
                        Ad = model.Ad,
                        YapimYili = model.YapimYili,
                        Ozet = model.Ozet,
                        AfisDosyaAdi = model.AfisDosyaAdi,
                        SureDakika = model.SureDakika,
                        YonetmenId = model.YonetmenId
                    };
                    await filmService.UpdateFilmAsync(guncellenecekFilm, model.TurIdleri, model.OyuncuIdleri, model.YonetmenId);

                    // Güncellenmiş filmi geri getir
                    var guncellenmiş = await filmService.GetFilmByIdAsync(id);
                    var response = ToResponseModel(guncellenmiş);
                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new CommonApiErrorResponseModel($"Film güncellenirken hata oluştu: {ex.Message}"));
                }
            });

            // DELETE /api/filmler/{id} - Film sil
            grup.MapDelete("/{id:int}", async (int id, IFilmService filmService) =>
            {
                try
                {
                    await filmService.DeleteFilmAsync(id);
                    return Results.Ok(new CommonApiResponseModel("Film başarıyla silindi."));
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new CommonApiErrorResponseModel($"Film silinirken hata oluştu: {ex.Message}"));
                }
            });

            // POST /api/filmler/{filmId}/kullanici/{kullaniciId}/puanla - Filme puan ver
            grup.MapPost("/{filmId:int}/kullanici/{kullaniciId:int}/puanla", async (int filmId, int kullaniciId, FilmPuanlaModel model, IFilmService filmService) =>
            {
                try
                {
                    var sonuc = await filmService.RateFilmAsync(kullaniciId, filmId, model.Puan);
                    return sonuc ? Results.Ok(new CommonApiResponseModel("Filme puan verildi.")) :
                                  Results.BadRequest(new CommonApiErrorResponseModel("Puanlama başarısız."));
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new CommonApiErrorResponseModel($"Puanlama sırasında hata oluştu: {ex.Message}"));
                }
            });

            // GET /api/filmler/{filmId}/ortalama-puan - Filmin ortalama puanını getir
            grup.MapGet("/{filmId:int}/ortalama-puan", async (int filmId, IFilmService filmService) =>
            {
                var ortalamaPuan = await filmService.GetFilmAverageRatingAsync(filmId);
                return ortalamaPuan.HasValue ? Results.Ok(new { filmId, ortalamaPuan = ortalamaPuan.Value }) :
                                              Results.NotFound(new CommonApiErrorResponseModel("Film için puan bulunamadı veya film yok."));
            });

            // GET /api/filmler/{filmId}/kullanici/{kullaniciId}/puan - Kullanıcının filme verdiği puanı getir
            grup.MapGet("/{filmId:int}/kullanici/{kullaniciId:int}/puan", async (int filmId, int kullaniciId, IFilmService filmService) =>
            {
                var kullaniciPuan = await filmService.GetUserRatingForFilmAsync(kullaniciId, filmId);
                return kullaniciPuan != null ? Results.Ok(kullaniciPuan) :
                                              Results.NotFound(new CommonApiErrorResponseModel("Kullanıcının bu filme verdiği bir puan bulunamadı."));
            });

            // GET /api/filmler/yonetmen/{yonetmenId} - Yönetmene göre filmleri getir
            grup.MapGet("/yonetmen/{yonetmenId:int}", async (int yonetmenId, IFilmService filmService) =>
            {
                var filmler = await filmService.GetFilmlerByYonetmenAsync(yonetmenId);
                var response = filmler.Select(ToResponseModel).ToList();
                return Results.Ok(response);
            });

            // GET /api/filmler/oyuncu/{oyuncuId} - Oyuncuya göre filmleri getir
            grup.MapGet("/oyuncu/{oyuncuId:int}", async (int oyuncuId, IFilmService filmService) =>
            {
                var filmler = await filmService.GetFilmlerByOyuncuAsync(oyuncuId);
                var response = filmler.Select(ToResponseModel).ToList();
                return Results.Ok(response);
            });

            // GET /api/filmler/tur/{turId} - Türe göre filmleri getir
            grup.MapGet("/tur/{turId:int}", async (int turId, IFilmService filmService) =>
            {
                var filmler = await filmService.GetFilmlerByTurAsync(turId);
                var response = filmler.Select(ToResponseModel).ToList();
                return Results.Ok(response);
            });
        }
    }
}