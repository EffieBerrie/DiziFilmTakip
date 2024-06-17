using DiziFilmTanitim.Core.Entities;
using DiziFilmTanitim.Core.Interfaces;
using DiziFilmTanitim.Api.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace DiziFilmTanitim.Api.Endpoints
{
    // Dizi için Modeller
    public record DiziEkleModel(
        string Ad,
        int? YapimYili,
        string? Ozet,
        string? AfisDosyaAdi,
        DiziDurumu Durum,
        int? YonetmenId,
        List<int>? TurIdleri,
        List<int>? OyuncuIdleri
    );

    public record DiziGuncelleModel(
        int Id,
        string Ad,
        int? YapimYili,
        string? Ozet,
        string? AfisDosyaAdi,
        DiziDurumu Durum,
        int? YonetmenId,
        List<int>? TurIdleri,
        List<int>? OyuncuIdleri
    );

    public record DiziPuanlaModel(int Puan);

    // Sezon için Modeller
    public record SezonEkleModel(int DiziId, int SezonNumarasi, string? Ad, DateTime? YayinTarihi);
    public record SezonGuncelleModel(int Id, int SezonNumarasi, string? Ad, DateTime? YayinTarihi, int DiziId);

    // Bölüm için Modeller
    public record BolumEkleModel(int SezonId, int BolumNumarasi, string? Ad, string? Ozet, DateTime? YayinTarihi, int? SureDakika);
    public record BolumGuncelleModel(int Id, int BolumNumarasi, string? Ad, string? Ozet, DateTime? YayinTarihi, int? SureDakika, int SezonId);

    // Dizi-specific Response Models

    public record BolumResponseModel(
        int Id,
        int BolumNumarasi,
        string? Ad,
        string? Ozet,
        DateTime? YayinTarihi,
        int? SureDakika
    );

    public record SezonResponseModel(
        int Id,
        int SezonNumarasi,
        string? Ad,
        DateTime? YayinTarihi,
        List<BolumResponseModel> Bolumler
    );

    public record DiziResponseModel(
        int Id,
        string Ad,
        int? YapimYili,
        string? Ozet,
        string? AfisDosyaAdi,
        DiziDurumu Durum,
        SimpleYonetmenResponseModel? Yonetmen,
        List<SimpleTurResponseModel> Turler,
        List<SimpleOyuncuResponseModel> Oyuncular,
        List<SezonResponseModel> Sezonlar
    );

    public static class DiziEndpoints
    {
        private static BolumResponseModel ToBolumResponseModel(Bolum? bolum)
        {
            if (bolum == null) throw new ArgumentNullException(nameof(bolum));

            return new BolumResponseModel(
                bolum.Id,
                bolum.BolumNumarasi,
                bolum.Ad,
                bolum.Ozet,
                bolum.YayinTarihi,
                bolum.SureDakika
            );
        }

        private static SezonResponseModel ToSezonResponseModel(Sezon? sezon)
        {
            if (sezon == null) throw new ArgumentNullException(nameof(sezon));

            return new SezonResponseModel(
                sezon.Id,
                sezon.SezonNumarasi,
                sezon.Ad,
                sezon.YayinTarihi,
                sezon.Bolumler?.Select(ToBolumResponseModel).ToList() ?? new List<BolumResponseModel>()
            );
        }

        private static DiziResponseModel ToDiziResponseModel(Dizi? dizi)
        {
            if (dizi == null) throw new ArgumentNullException(nameof(dizi));

            return new DiziResponseModel(
                dizi.Id,
                dizi.Ad,
                dizi.YapimYili,
                dizi.Ozet,
                dizi.AfisDosyaAdi,
                dizi.Durum,
                dizi.Yonetmen != null ? new SimpleYonetmenResponseModel(dizi.Yonetmen.Id, dizi.Yonetmen.AdSoyad) : null,
                dizi.Turler?.Select(t => new SimpleTurResponseModel(t.Id, t.Ad)).ToList() ?? new List<SimpleTurResponseModel>(),
                dizi.Oyuncular?.Select(o => new SimpleOyuncuResponseModel(o.Id, o.AdSoyad)).ToList() ?? new List<SimpleOyuncuResponseModel>(),
                dizi.Sezonlar?.Select(ToSezonResponseModel).ToList() ?? new List<SezonResponseModel>()
            );
        }

        public static void MapDiziEndpoints(this IEndpointRouteBuilder app)
        {
            var grup = app.MapGroup("/api/diziler").WithTags("Dizi İşlemleri");

            // === Dizi Endpointleri ===
            grup.MapGet("/", async (IDiziService diziService, string? arama = null) =>
            {
                var diziler = await diziService.GetAllDizilerSimpleAsync();

                // Arama varsa filtrele
                if (!string.IsNullOrWhiteSpace(arama))
                {
                    diziler = diziler.Where(d => d.Ad.Contains(arama, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                var response = new { Success = true, Data = diziler.Select(ToDiziResponseModel).ToList() };
                return Results.Ok(response);
            });

            grup.MapGet("/ara", async (IDiziService diziService,
                string? ad = null,
                int? yapimYili = null,
                int? turId = null,
                int? yonetmenId = null,
                int? oyuncuId = null,
                DiziDurumu? durum = null) =>
            {
                try
                {
                    var tumDiziler = await diziService.GetAllDizilerSimpleAsync();
                    var query = tumDiziler.AsQueryable();

                    // Filtreleme kriterleri
                    if (!string.IsNullOrWhiteSpace(ad))
                        query = query.Where(d => d.Ad.Contains(ad, StringComparison.OrdinalIgnoreCase));

                    if (yapimYili.HasValue)
                        query = query.Where(d => d.YapimYili == yapimYili.Value);

                    if (turId.HasValue)
                        query = query.Where(d => d.Turler.Any(t => t.Id == turId.Value));

                    if (yonetmenId.HasValue)
                        query = query.Where(d => d.YonetmenId == yonetmenId.Value);

                    if (oyuncuId.HasValue)
                        query = query.Where(d => d.Oyuncular.Any(o => o.Id == oyuncuId.Value));

                    if (durum.HasValue)
                        query = query.Where(d => d.Durum == durum.Value);

                    var filtrelenmisDiziler = query.ToList();
                    var response = filtrelenmisDiziler.Select(ToDiziResponseModel);
                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Diziler aranırken hata oluştu: {ex.Message}");
                }
            });

            grup.MapGet("/{id:int}", async (int id, IDiziService diziService) =>
            {
                var dizi = await diziService.GetDiziByIdAsync(id);
                if (dizi == null) return Results.NotFound(new CommonApiErrorResponseModel("Dizi bulunamadı."));

                var response = ToDiziResponseModel(dizi);
                return Results.Ok(response);
            });

            grup.MapPost("/", async (DiziEkleModel model, IDiziService diziService) =>
            {
                var yeniDizi = new Dizi
                {
                    Ad = model.Ad,
                    YapimYili = model.YapimYili,
                    Ozet = model.Ozet,
                    AfisDosyaAdi = model.AfisDosyaAdi,
                    Durum = model.Durum,
                    YonetmenId = model.YonetmenId
                };
                var olusturulanDizi = await diziService.AddDiziAsync(yeniDizi, model.TurIdleri, model.OyuncuIdleri, model.YonetmenId);
                var response = ToDiziResponseModel(olusturulanDizi);
                return Results.Created($"/api/diziler/{olusturulanDizi.Id}", response);
            });

            grup.MapPut("/{id:int}", async (int id, DiziGuncelleModel model, IDiziService diziService) =>
            {
                if (id != model.Id) return Results.BadRequest("ID uyuşmazlığı.");
                var guncellenecekDizi = new Dizi
                {
                    Id = model.Id,
                    Ad = model.Ad,
                    YapimYili = model.YapimYili,
                    Ozet = model.Ozet,
                    AfisDosyaAdi = model.AfisDosyaAdi,
                    Durum = model.Durum,
                    YonetmenId = model.YonetmenId
                };
                await diziService.UpdateDiziAsync(guncellenecekDizi, model.TurIdleri, model.OyuncuIdleri, model.YonetmenId);

                // Güncellenmiş diziyi geri getir
                var guncellenmiş = await diziService.GetDiziByIdAsync(id);
                var response = ToDiziResponseModel(guncellenmiş);
                return Results.Ok(response);
            });

            grup.MapDelete("/{id:int}", async (int id, IDiziService diziService) =>
            {
                try
                {
                    await diziService.DeleteDiziAsync(id);
                    return Results.Ok(new CommonApiResponseModel("Dizi başarıyla silindi."));
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new CommonApiErrorResponseModel($"Dizi silinirken hata oluştu: {ex.Message}"));
                }
            });

            grup.MapPost("/{diziId:int}/kullanici/{kullaniciId:int}/puanla", async (int diziId, int kullaniciId, DiziPuanlaModel model, IDiziService diziService) =>
            {
                var sonuc = await diziService.RateDiziAsync(kullaniciId, diziId, model.Puan);
                return sonuc ? Results.Ok(new { message = "Diziye puan verildi." }) : Results.BadRequest("Puanlama başarısız.");
            });

            grup.MapGet("/{diziId:int}/ortalama-puan", async (int diziId, IDiziService diziService) =>
            {
                var ortalamaPuan = await diziService.GetDiziAverageRatingAsync(diziId);
                return ortalamaPuan.HasValue ? Results.Ok(new { diziId, ortalamaPuan = ortalamaPuan.Value }) : Results.NotFound("Dizi için puan bulunamadı veya dizi yok.");
            });

            grup.MapGet("/{diziId:int}/kullanici/{kullaniciId:int}/puan", async (int diziId, int kullaniciId, IDiziService diziService) =>
            {
                var kullaniciPuan = await diziService.GetUserRatingForDiziAsync(kullaniciId, diziId);
                return kullaniciPuan != null ? Results.Ok(kullaniciPuan) : Results.NotFound("Kullanıcının bu diziye verdiği bir puan bulunamadı.");
            });

            grup.MapGet("/yonetmen/{yonetmenId:int}", async (int yonetmenId, IDiziService diziService) =>
            {
                var diziler = await diziService.GetDizilerByYonetmenAsync(yonetmenId);
                var response = diziler.Select(ToDiziResponseModel).ToList();
                return Results.Ok(response);
            });

            grup.MapGet("/oyuncu/{oyuncuId:int}", async (int oyuncuId, IDiziService diziService) =>
            {
                var diziler = await diziService.GetDizilerByOyuncuAsync(oyuncuId);
                var response = diziler.Select(ToDiziResponseModel).ToList();
                return Results.Ok(response);
            });

            grup.MapGet("/tur/{turId:int}", async (int turId, IDiziService diziService) =>
            {
                var diziler = await diziService.GetDizilerByTurAsync(turId);
                var response = diziler.Select(ToDiziResponseModel).ToList();
                return Results.Ok(response);
            });

            // === Sezon Endpointleri ===
            var sezonGrup = grup.MapGroup("/{diziId:int}/sezonlar"); // /api/diziler/{diziId}/sezonlar

            sezonGrup.MapGet("/", async (int diziId, IDiziService diziService) =>
            {
                var sezonlar = await diziService.GetSezonlarByDiziAsync(diziId);
                var response = sezonlar.Select(ToSezonResponseModel).ToList();
                return Results.Ok(response);
            });

            sezonGrup.MapGet("/{sezonId:int}", async (int sezonId, IDiziService diziService) =>
            {
                var sezon = await diziService.GetSezonByIdAsync(sezonId);
                if (sezon == null) return Results.NotFound("Sezon bulunamadı.");

                var response = ToSezonResponseModel(sezon);
                return Results.Ok(response);
            });

            sezonGrup.MapPost("/", async (int diziId, SezonEkleModel model, IDiziService diziService) =>
            {
                if (diziId != model.DiziId) return Results.BadRequest("URL dizi ID ile modeldeki Dizi ID eşleşmiyor.");
                var yeniSezon = new Sezon { DiziId = model.DiziId, SezonNumarasi = model.SezonNumarasi, Ad = model.Ad, YayinTarihi = model.YayinTarihi };
                var olusturulan = await diziService.AddSezonAsync(diziId, yeniSezon);
                var response = ToSezonResponseModel(olusturulan);
                return Results.Created($"/api/diziler/{diziId}/sezonlar/{olusturulan.Id}", response);
            });

            sezonGrup.MapPut("/{sezonId:int}", async (int diziId, int sezonId, SezonGuncelleModel model, IDiziService diziService) =>
            {
                if (sezonId != model.Id || diziId != model.DiziId) return Results.BadRequest("ID uyuşmazlığı.");
                var guncellenecekSezon = new Sezon { Id = model.Id, DiziId = model.DiziId, SezonNumarasi = model.SezonNumarasi, Ad = model.Ad, YayinTarihi = model.YayinTarihi };
                await diziService.UpdateSezonAsync(guncellenecekSezon);

                // Güncellenmiş sezonu geri getir
                var guncellenmiş = await diziService.GetSezonByIdAsync(sezonId);
                var response = ToSezonResponseModel(guncellenmiş);
                return Results.Ok(response);
            });

            sezonGrup.MapDelete("/{sezonId:int}", async (int sezonId, IDiziService diziService) =>
            {
                await diziService.DeleteSezonAsync(sezonId);
                return Results.Ok(new { message = "Sezon silindi." });
            });

            // === Bölüm Endpointleri ===
            var bolumGrup = app.MapGroup("/api/sezonlar/{sezonId:int}/bolumler").WithTags("Bölüm İşlemleri"); // Ayrı bir grup olarak tanımlandı

            bolumGrup.MapGet("/", async (int sezonId, IDiziService diziService) =>
            {
                var bolumler = await diziService.GetBolumlerBySezonAsync(sezonId);
                var response = bolumler.Select(ToBolumResponseModel).ToList();
                return Results.Ok(response);
            });

            bolumGrup.MapGet("/{bolumId:int}", async (int bolumId, IDiziService diziService) =>
            {
                var bolum = await diziService.GetBolumByIdAsync(bolumId);
                if (bolum == null) return Results.NotFound("Bölüm bulunamadı.");

                var response = ToBolumResponseModel(bolum);
                return Results.Ok(response);
            });

            bolumGrup.MapPost("/", async (int sezonId, BolumEkleModel model, IDiziService diziService) =>
            {
                if (sezonId != model.SezonId) return Results.BadRequest("URL sezon ID ile modeldeki Sezon ID eşleşmiyor.");
                var yeniBolum = new Bolum { SezonId = model.SezonId, BolumNumarasi = model.BolumNumarasi, Ad = model.Ad, Ozet = model.Ozet, YayinTarihi = model.YayinTarihi, SureDakika = model.SureDakika };
                var olusturulan = await diziService.AddBolumAsync(sezonId, yeniBolum);
                var response = ToBolumResponseModel(olusturulan);
                return Results.Created($"/api/sezonlar/{sezonId}/bolumler/{olusturulan.Id}", response);
            });

            bolumGrup.MapPut("/{bolumId:int}", async (int sezonId, int bolumId, BolumGuncelleModel model, IDiziService diziService) =>
            {
                if (bolumId != model.Id || sezonId != model.SezonId) return Results.BadRequest("ID uyuşmazlığı.");
                var guncellenecekBolum = new Bolum { Id = model.Id, SezonId = model.SezonId, BolumNumarasi = model.BolumNumarasi, Ad = model.Ad, Ozet = model.Ozet, YayinTarihi = model.YayinTarihi, SureDakika = model.SureDakika };
                await diziService.UpdateBolumAsync(guncellenecekBolum);

                // Güncellenmiş bölümü geri getir
                var guncellenmiş = await diziService.GetBolumByIdAsync(bolumId);
                var response = ToBolumResponseModel(guncellenmiş);
                return Results.Ok(response);
            });

            bolumGrup.MapDelete("/{bolumId:int}", async (int bolumId, IDiziService diziService) =>
            {
                await diziService.DeleteBolumAsync(bolumId);
                return Results.Ok(new { message = "Bölüm silindi." });
            });
        }
    }
}