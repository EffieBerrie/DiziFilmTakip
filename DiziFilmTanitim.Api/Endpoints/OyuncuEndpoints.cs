using DiziFilmTanitim.Core.Entities;
using DiziFilmTanitim.Core.Interfaces;
using DiziFilmTanitim.Api.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace DiziFilmTanitim.Api.Endpoints
{
    public record OyuncuModel(string AdSoyad, DateTime? DogumTarihi, string? Biyografi, string? FotografDosyaAdi);

    public static class OyuncuEndpoints
    {
        private static CommonOyuncuResponseModel ToResponseModel(Oyuncu oyuncu)
        {
            return new CommonOyuncuResponseModel(
                oyuncu.Id,
                oyuncu.AdSoyad,
                oyuncu.DogumTarihi,
                oyuncu.Biyografi,
                oyuncu.FotografDosyaAdi
            );
        }

        public static void MapOyuncuEndpoints(this IEndpointRouteBuilder app)
        {
            var grup = app.MapGroup("/api/oyuncular").WithTags("Oyuncu İşlemleri");

            // GET /api/oyuncular - Tüm oyuncuları getir (aramalı)
            grup.MapGet("/", async (IOyuncuService oyuncuService, string? aramaKelimesi = null) =>
            {
                var oyuncular = await oyuncuService.GetAllOyuncularAsync(aramaKelimesi);
                var response = oyuncular.Select(ToResponseModel).ToList();
                return Results.Ok(response);
            });

            // GET /api/oyuncular/{id} - ID ile oyuncu getir
            grup.MapGet("/{id:int}", async (int id, IOyuncuService oyuncuService) =>
            {
                var oyuncu = await oyuncuService.GetOyuncuByIdAsync(id);
                if (oyuncu == null) return Results.NotFound(new CommonApiErrorResponseModel("Oyuncu bulunamadı."));

                var response = ToResponseModel(oyuncu);
                return Results.Ok(response);
            });

            // POST /api/oyuncular - Yeni oyuncu ekle
            grup.MapPost("/", async (OyuncuModel model, IOyuncuService oyuncuService) =>
            {
                try
                {
                    var yeniOyuncu = new Oyuncu
                    {
                        AdSoyad = model.AdSoyad,
                        DogumTarihi = model.DogumTarihi,
                        Biyografi = model.Biyografi,
                        FotografDosyaAdi = model.FotografDosyaAdi
                    };
                    var olusturulanOyuncu = await oyuncuService.AddOyuncuAsync(yeniOyuncu);
                    var response = ToResponseModel(olusturulanOyuncu);
                    return Results.Created($"/api/oyuncular/{olusturulanOyuncu.Id}", response);
                }
                catch (System.Exception ex)
                {
                    return Results.BadRequest(new CommonApiErrorResponseModel($"Oyuncu eklenirken bir hata oluştu: {ex.Message}"));
                }
            });

            // PUT /api/oyuncular/{id} - Oyuncu güncelle
            grup.MapPut("/{id:int}", async (int id, OyuncuModel model, IOyuncuService oyuncuService) =>
            {
                try
                {
                    var mevcutOyuncu = await oyuncuService.GetOyuncuByIdAsync(id);
                    if (mevcutOyuncu == null) return Results.NotFound(new CommonApiErrorResponseModel("Güncellenecek oyuncu bulunamadı."));

                    mevcutOyuncu.AdSoyad = model.AdSoyad;
                    mevcutOyuncu.DogumTarihi = model.DogumTarihi;
                    mevcutOyuncu.Biyografi = model.Biyografi;
                    mevcutOyuncu.FotografDosyaAdi = model.FotografDosyaAdi;

                    await oyuncuService.UpdateOyuncuAsync(mevcutOyuncu);
                    var response = ToResponseModel(mevcutOyuncu);
                    return Results.Ok(response);
                }
                catch (System.Exception ex)
                {
                    return Results.BadRequest(new CommonApiErrorResponseModel($"Oyuncu güncellenirken bir hata oluştu: {ex.Message}"));
                }
            });

            // DELETE /api/oyuncular/{id} - Oyuncu sil
            grup.MapDelete("/{id:int}", async (int id, IOyuncuService oyuncuService) =>
            {
                try
                {
                    var oyuncu = await oyuncuService.GetOyuncuByIdAsync(id); // Önce varlığını kontrol edelim
                    if (oyuncu == null) return Results.NotFound(new CommonApiErrorResponseModel("Silinecek oyuncu bulunamadı."));

                    await oyuncuService.DeleteOyuncuAsync(id);
                    return Results.Ok(new CommonApiResponseModel("Oyuncu başarıyla silindi."));
                }
                catch (System.InvalidOperationException ex) // Servis oyuncuya bağlı film/dizi varsa hata fırlatırsa
                {
                    return Results.BadRequest(new CommonApiErrorResponseModel(ex.Message));
                }
                catch (System.Exception ex)
                {
                    return Results.Problem($"Oyuncu silinirken bir sorun oluştu: {ex.Message}");
                }
            });
        }
    }
}