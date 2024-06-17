using DiziFilmTanitim.Core.Entities;
using DiziFilmTanitim.Core.Interfaces;
using DiziFilmTanitim.Api.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;
using System.Linq;

namespace DiziFilmTanitim.Api.Endpoints
{
    public record TurModel(string Ad);

    public static class TurEndpoints
    {
        private static CommonTurResponseModel ToResponseModel(Tur tur)
        {
            return new CommonTurResponseModel(tur.Id, tur.Ad);
        }

        public static void MapTurEndpoints(this IEndpointRouteBuilder app)
        {
            var grup = app.MapGroup("/api/turler").WithTags("Tür İşlemleri");

            // GET /api/turler - Tüm türleri getir (arama filtreli)
            grup.MapGet("/", async (ITurService turService, string? aramaKelimesi = null) =>
            {
                var turler = await turService.GetAllTurlerAsync(aramaKelimesi);
                var response = turler.Select(ToResponseModel).ToList();
                return Results.Ok(response);
            });

            // GET /api/turler/{id} - ID ile tür getir
            grup.MapGet("/{id:int}", async (int id, ITurService turService) =>
            {
                var tur = await turService.GetTurByIdAsync(id);
                if (tur == null) return Results.NotFound(new CommonApiErrorResponseModel("Tür bulunamadı."));

                var response = ToResponseModel(tur);
                return Results.Ok(response);
            });

            // POST /api/turler - Yeni tür ekle
            grup.MapPost("/", async (TurModel model, ITurService turService) =>
            {
                try
                {
                    var yeniTur = new Tur { Ad = model.Ad };
                    var olusturulanTur = await turService.AddTurAsync(yeniTur);
                    var response = ToResponseModel(olusturulanTur);
                    return Results.Created($"/api/turler/{olusturulanTur.Id}", response);
                }
                catch (System.InvalidOperationException ex) // Servis mevcut tür için hata fırlatırsa
                {
                    return Results.Conflict(new CommonApiErrorResponseModel(ex.Message));
                }
                catch (System.Exception ex)
                {
                    return Results.BadRequest(new CommonApiErrorResponseModel($"Tür eklenirken bir hata oluştu: {ex.Message}"));
                }
            });

            // PUT /api/turler/{id} - Tür güncelle
            grup.MapPut("/{id:int}", async (int id, TurModel model, ITurService turService) =>
            {
                try
                {
                    var mevcutTur = await turService.GetTurByIdAsync(id);
                    if (mevcutTur == null) return Results.NotFound(new CommonApiErrorResponseModel("Güncellenecek tür bulunamadı."));

                    mevcutTur.Ad = model.Ad;
                    await turService.UpdateTurAsync(mevcutTur);
                    var response = ToResponseModel(mevcutTur);
                    return Results.Ok(response);
                }
                catch (System.InvalidOperationException ex) // Servis aynı isimde başka tür için hata fırlatırsa
                {
                    return Results.Conflict(new CommonApiErrorResponseModel(ex.Message));
                }
                catch (System.Exception ex)
                {
                    return Results.BadRequest(new CommonApiErrorResponseModel($"Tür güncellenirken bir hata oluştu: {ex.Message}"));
                }
            });

            // DELETE /api/turler/{id} - Tür sil
            grup.MapDelete("/{id:int}", async (int id, ITurService turService) =>
            {
                try
                {
                    await turService.DeleteTurAsync(id);
                    return Results.Ok(new CommonApiResponseModel("Tür başarıyla silindi."));
                }
                catch (System.InvalidOperationException ex) // Servis türe bağlı film/dizi varsa hata fırlatırsa
                {
                    return Results.BadRequest(new CommonApiErrorResponseModel(ex.Message));
                }
                catch (System.Exception ex)
                {
                    return Results.Problem($"Tür silinirken bir sorun oluştu: {ex.Message}");
                }
            });
        }
    }
}