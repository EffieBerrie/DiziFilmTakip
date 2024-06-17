using DiziFilmTanitim.Core.Entities;
using DiziFilmTanitim.Core.Interfaces;
using DiziFilmTanitim.Api.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;
using System;

namespace DiziFilmTanitim.Api.Endpoints
{
    public record YonetmenModel(string AdSoyad, DateTime? DogumTarihi, string? Biyografi, string? FotografDosyaAdi);

    public static class YonetmenEndpoints
    {
        private static CommonYonetmenResponseModel ToResponseModel(Yonetmen yonetmen)
        {
            return new CommonYonetmenResponseModel(
                yonetmen.Id,
                yonetmen.AdSoyad,
                yonetmen.DogumTarihi,
                yonetmen.Biyografi,
                yonetmen.FotografDosyaAdi
            );
        }

        public static void MapYonetmenEndpoints(this IEndpointRouteBuilder app)
        {
            var grup = app.MapGroup("/api/yonetmenler").WithTags("Yönetmen İşlemleri");

            // GET /api/yonetmenler - Tüm yönetmenleri getir (aramalı)
            grup.MapGet("/", async (IYonetmenService yonetmenService, string? aramaKelimesi = null) =>
            {
                var yonetmenler = await yonetmenService.GetAllYonetmenlerAsync(aramaKelimesi);
                var response = yonetmenler.Select(ToResponseModel).ToList();
                return Results.Ok(response);
            });

            // GET /api/yonetmenler/{id} - ID ile yönetmen getir
            grup.MapGet("/{id:int}", async (int id, IYonetmenService yonetmenService) =>
            {
                var yonetmen = await yonetmenService.GetYonetmenByIdAsync(id);
                if (yonetmen == null) return Results.NotFound(new CommonApiErrorResponseModel("Yönetmen bulunamadı."));

                var response = ToResponseModel(yonetmen);
                return Results.Ok(response);
            });

            // POST /api/yonetmenler - Yeni yönetmen ekle
            grup.MapPost("/", async (YonetmenModel model, IYonetmenService yonetmenService) =>
            {
                try
                {
                    var yeniYonetmen = new Yonetmen
                    {
                        AdSoyad = model.AdSoyad,
                        DogumTarihi = model.DogumTarihi,
                        Biyografi = model.Biyografi,
                        FotografDosyaAdi = model.FotografDosyaAdi
                    };
                    var olusturulanYonetmen = await yonetmenService.AddYonetmenAsync(yeniYonetmen);
                    var response = ToResponseModel(olusturulanYonetmen);
                    return Results.Created($"/api/yonetmenler/{olusturulanYonetmen.Id}", response);
                }
                catch (System.Exception ex)
                {
                    return Results.BadRequest(new CommonApiErrorResponseModel($"Yönetmen eklenirken bir hata oluştu: {ex.Message}"));
                }
            });

            // PUT /api/yonetmenler/{id} - Yönetmen güncelle
            grup.MapPut("/{id:int}", async (int id, YonetmenModel model, IYonetmenService yonetmenService) =>
            {
                try
                {
                    var mevcutYonetmen = await yonetmenService.GetYonetmenByIdAsync(id);
                    if (mevcutYonetmen == null) return Results.NotFound(new CommonApiErrorResponseModel("Güncellenecek yönetmen bulunamadı."));

                    mevcutYonetmen.AdSoyad = model.AdSoyad;
                    mevcutYonetmen.DogumTarihi = model.DogumTarihi;
                    mevcutYonetmen.Biyografi = model.Biyografi;
                    mevcutYonetmen.FotografDosyaAdi = model.FotografDosyaAdi;

                    await yonetmenService.UpdateYonetmenAsync(mevcutYonetmen);
                    var response = ToResponseModel(mevcutYonetmen);
                    return Results.Ok(response);
                }
                catch (System.Exception ex)
                {
                    return Results.BadRequest(new CommonApiErrorResponseModel($"Yönetmen güncellenirken bir hata oluştu: {ex.Message}"));
                }
            });

            // DELETE /api/yonetmenler/{id} - Yönetmen sil
            grup.MapDelete("/{id:int}", async (int id, IYonetmenService yonetmenService) =>
            {
                try
                {
                    var yonetmen = await yonetmenService.GetYonetmenByIdAsync(id); // Önce varlığını kontrol edelim
                    if (yonetmen == null) return Results.NotFound(new CommonApiErrorResponseModel("Silinecek yönetmen bulunamadı."));

                    await yonetmenService.DeleteYonetmenAsync(id);
                    return Results.Ok(new CommonApiResponseModel("Yönetmen başarıyla silindi."));
                }
                catch (System.InvalidOperationException ex) // Servis yönetmene bağlı film/dizi varsa hata fırlatırsa
                {
                    return Results.BadRequest(new CommonApiErrorResponseModel(ex.Message));
                }
                catch (System.Exception ex)
                {
                    return Results.Problem($"Yönetmen silinirken bir sorun oluştu: {ex.Message}");
                }
            });
        }
    }
}