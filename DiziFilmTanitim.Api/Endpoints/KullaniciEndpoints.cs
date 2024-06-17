using DiziFilmTanitim.Core.Entities;
using DiziFilmTanitim.Core.Interfaces;
using DiziFilmTanitim.Api.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;
using System;

// using DiziFilmTanitim.Api.Services; // IKullaniciService üzerinden erişildiği için bu genellikle gereksizdir.

namespace DiziFilmTanitim.Api.Endpoints
{
    // Endpointler için kullanılan yardımcı record (kayıt) tipleri
    public record KullaniciKayitModel(string KullaniciAdi, string Eposta, string Sifre);
    public record LoginModel(string KullaniciAdi, string Sifre);
    public record SifreDegistirModel(string EskiSifre, string YeniSifre);
    public record KullaniciGuncelleModel(string? Email); // Kullanici entity'sindeki 'Email' alanıyla tutarlı

    // Response modelleri
    public record KullaniciResponseModel(int Id, string KullaniciAdi, string? Email);
    public record LoginResponseModel(int Id, string KullaniciAdi, string? Email, string Message = "Giriş başarılı");

    public static class KullaniciEndpoints
    {
        public static void MapKullaniciEndpoints(this IEndpointRouteBuilder app)
        {
            var grup = app.MapGroup("/api/kullanicilar").WithTags("Kullanıcı İşlemleri");

            // GET /api/kullanicilar - Tüm kullanıcıları listele
            grup.MapGet("/", async (IKullaniciService kullaniciService) =>
            {
                var kullanicilar = await kullaniciService.GetAllKullanicilarAsync();
                var response = kullanicilar.Select(k => new KullaniciResponseModel(k.Id, k.KullaniciAdi, k.Email));
                return Results.Ok(response);
            });

            // POST /api/kullanicilar/kayit - Yeni kullanıcı kaydı
            grup.MapPost("/kayit", async (KullaniciKayitModel model, IKullaniciService kullaniciService) =>
            {
                try
                {
                    var kullanici = new Kullanici { KullaniciAdi = model.KullaniciAdi, Email = model.Eposta, Sifre = model.Sifre };
                    var olusturulanKullanici = await kullaniciService.RegisterAsync(kullanici);
                    var response = new KullaniciResponseModel(olusturulanKullanici.Id, olusturulanKullanici.KullaniciAdi, olusturulanKullanici.Email);
                    return Results.Created($"/api/kullanicilar/{olusturulanKullanici.Id}", response);
                }
                catch (System.InvalidOperationException ex)
                {
                    return Results.Conflict(new CommonApiErrorResponseModel(ex.Message));
                }
                catch (System.Exception ex)
                {
                    return Results.BadRequest(new CommonApiErrorResponseModel($"Kullanıcı kaydedilirken hata oluştu: {ex.Message}"));
                }
            });

            // POST /api/kullanicilar/giris - Kullanıcı girişi
            grup.MapPost("/giris", async (LoginModel model, IKullaniciService kullaniciService) =>
            {
                var kullanici = await kullaniciService.LoginAsync(model.KullaniciAdi, model.Sifre);
                if (kullanici == null)
                {
                    return Results.Unauthorized();
                }
                var response = new LoginResponseModel(kullanici.Id, kullanici.KullaniciAdi, kullanici.Email);
                return Results.Ok(response);
            });

            // POST /api/kullanicilar/cikis/{kullaniciId} - Kullanıcı çıkışı (logout)
            grup.MapPost("/cikis/{kullaniciId:int}", async (int kullaniciId, IKullaniciService kullaniciService) =>
            {
                var sonuc = await kullaniciService.LogoutAsync(kullaniciId);
                if (!sonuc)
                {
                    return Results.NotFound(new CommonApiResponseModel("Kullanıcı bulunamadı.", false));
                }

                var response = new CommonApiResponseModel("Başarıyla çıkış yapıldı.", true);
                return Results.Ok(response);
            });

            // PUT /api/kullanicilar/{id}/sifre-degistir - Kullanıcı şifre değiştirme
            grup.MapPut("/{id:int}/sifre-degistir", async (int id, SifreDegistirModel model, IKullaniciService kullaniciService) =>
            {
                try
                {
                    var sonuc = await kullaniciService.ChangePasswordAsync(id, model.EskiSifre, model.YeniSifre);
                    if (sonuc)
                    {
                        var response = new CommonApiResponseModel("Şifre başarıyla değiştirildi.", true);
                        return Results.Ok(response);
                    }
                    else
                    {
                        return Results.BadRequest(new CommonApiResponseModel("Şifre değiştirilemedi.", false));
                    }
                }
                catch (ArgumentException ex)
                {
                    // Kullanıcı bulunamadı
                    return Results.NotFound(new CommonApiResponseModel(ex.Message, false));
                }
                catch (InvalidOperationException ex)
                {
                    // Eski şifre yanlış
                    return Results.BadRequest(new CommonApiResponseModel(ex.Message, false));
                }
                catch (Exception ex)
                {
                    // Genel hata
                    return Results.BadRequest(new CommonApiResponseModel($"Şifre değiştirme sırasında bir hata oluştu: {ex.Message}", false));
                }
            });

            // GET /api/kullanicilar/{id} - ID ile kullanıcı getir
            grup.MapGet("/{id:int}", async (int id, IKullaniciService kullaniciService) =>
            {
                var kullanici = await kullaniciService.GetKullaniciByIdAsync(id);
                if (kullanici == null) return Results.NotFound(new CommonApiErrorResponseModel("Kullanıcı bulunamadı."));

                var response = new KullaniciResponseModel(kullanici.Id, kullanici.KullaniciAdi, kullanici.Email);
                return Results.Ok(response);
            });

            // GET /api/kullanicilar/kullaniciadi/{kullaniciAdi} - Kullanıcı adına göre kullanıcı getir
            grup.MapGet("/kullaniciadi/{kullaniciAdi}", async (string kullaniciAdi, IKullaniciService kullaniciService) =>
            {
                var kullanici = await kullaniciService.GetKullaniciByKullaniciAdiAsync(kullaniciAdi);
                if (kullanici == null) return Results.NotFound(new CommonApiErrorResponseModel("Kullanıcı bulunamadı."));

                var response = new KullaniciResponseModel(kullanici.Id, kullanici.KullaniciAdi, kullanici.Email);
                return Results.Ok(response);
            });

            // PUT /api/kullanicilar/{id} - Kullanıcı bilgilerini güncelle
            grup.MapPut("/{id:int}", async (int id, KullaniciGuncelleModel model, IKullaniciService kullaniciService) =>
            {
                var mevcutKullanici = await kullaniciService.GetKullaniciByIdAsync(id);
                if (mevcutKullanici == null)
                {
                    return Results.NotFound(new CommonApiResponseModel("Güncellenecek kullanıcı bulunamadı.", false));
                }

                mevcutKullanici.Email = model.Email;

                var sonuc = await kullaniciService.UpdateKullaniciAsync(mevcutKullanici);
                if (!sonuc)
                {
                    return Results.BadRequest(new CommonApiResponseModel("Kullanıcı bilgileri güncellenemedi.", false));
                }

                var response = new CommonApiResponseModel("Kullanıcı bilgileri başarıyla güncellendi.", true);
                return Results.Ok(response);
            });

            // DELETE /api/kullanicilar/{id} - Kullanıcı sil
            grup.MapDelete("/{id:int}", async (int id, IKullaniciService kullaniciService) =>
            {
                var kullanici = await kullaniciService.GetKullaniciByIdAsync(id);
                if (kullanici == null)
                {
                    return Results.NotFound(new CommonApiResponseModel("Kullanıcı bulunamadı.", false));
                }

                var sonuc = await kullaniciService.DeleteKullaniciAsync(id);
                if (!sonuc)
                {
                    return Results.BadRequest(new CommonApiResponseModel("Kullanıcı silinemedi.", false));
                }

                var response = new CommonApiResponseModel("Kullanıcı başarıyla silindi.", true);
                return Results.Ok(response);
            });
        }
    }
}