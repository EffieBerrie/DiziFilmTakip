using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using DiziFilmTanitim.Api.Interfaces;

namespace DiziFilmTanitim.Api.Endpoints
{
    public static class UploadEndpoints
    {
        public static void MapUploadEndpoints(this IEndpointRouteBuilder app)
        {
            var grup = app.MapGroup("/api/upload").WithTags("Dosya Yükleme");

            // Film afişi yükleme
            grup.MapPost("/film/{filmId}/afis", async (int filmId, IFormFile file, IUploadService uploadService) =>
            {
                var result = await uploadService.UploadFilmAfisAsync(filmId, file);

                if (!result.success)
                    return Results.BadRequest(new { success = false, message = result.message });

                return Results.Ok(new
                {
                    success = true,
                    fileName = result.fileName,
                    url = uploadService.GetFileUrl(result.fileName, "afisler"),
                    message = result.message
                });
            }).DisableAntiforgery();

            // Dizi afişi yükleme
            grup.MapPost("/dizi/{diziId}/afis", async (int diziId, IFormFile file, IUploadService uploadService) =>
            {
                var result = await uploadService.UploadDiziAfisAsync(diziId, file);

                if (!result.success)
                    return Results.BadRequest(new { success = false, message = result.message });

                return Results.Ok(new
                {
                    success = true,
                    fileName = result.fileName,
                    url = uploadService.GetFileUrl(result.fileName, "afisler"),
                    message = result.message
                });
            }).DisableAntiforgery();

            // Oyuncu fotoğrafı yükleme
            grup.MapPost("/oyuncu/{oyuncuId}/fotograf", async (int oyuncuId, IFormFile file, IUploadService uploadService) =>
            {
                var result = await uploadService.UploadOyuncuFotografAsync(oyuncuId, file);

                if (!result.success)
                    return Results.BadRequest(new { success = false, message = result.message });

                return Results.Ok(new
                {
                    success = true,
                    fileName = result.fileName,
                    url = uploadService.GetFileUrl(result.fileName, "fotograflar"),
                    message = result.message
                });
            }).DisableAntiforgery();

            // Yönetmen fotoğrafı yükleme
            grup.MapPost("/yonetmen/{yonetmenId}/fotograf", async (int yonetmenId, IFormFile file, IUploadService uploadService) =>
            {
                var result = await uploadService.UploadYonetmenFotografAsync(yonetmenId, file);

                if (!result.success)
                    return Results.BadRequest(new { success = false, message = result.message });

                return Results.Ok(new
                {
                    success = true,
                    fileName = result.fileName,
                    url = uploadService.GetFileUrl(result.fileName, "fotograflar"),
                    message = result.message
                });
            }).DisableAntiforgery();
        }
    }
}