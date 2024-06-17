using DiziFilmTanitim.Core.Entities;
using DiziFilmTanitim.Core.Interfaces;
using DiziFilmTanitim.Api.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace DiziFilmTanitim.Api.Endpoints
{
    public record KullaniciListesiEkleModel(string ListeAdi, string? Aciklama);
    public record KullaniciListesiGuncelleModel(int Id, string ListeAdi, string? Aciklama, int KullaniciId);
    public record ListeIcerikModel(int IcerikId); // FilmId veya DiziId için genel model

    // Response model'ler
    public record KullaniciListesiResponseModel(int Id, string ListeAdi, string? Aciklama, int KullaniciId);
    public record ListeFilmResponseModel(int Id, string Ad, int? YapimYili, string? AfisDosyaAdi);
    public record ListeDiziResponseModel(int Id, string Ad, int? YapimYili, string? AfisDosyaAdi, DiziDurumu Durum);

    public static class KullaniciListesiEndpoints
    {
        private static KullaniciListesiResponseModel ToListeResponseModel(KullaniciListesi? liste)
        {
            if (liste == null) throw new ArgumentNullException(nameof(liste));
            return new KullaniciListesiResponseModel(liste.Id, liste.ListeAdi, liste.Aciklama, liste.KullaniciId);
        }

        private static ListeFilmResponseModel ToFilmResponseModel(Film? film)
        {
            if (film == null) throw new ArgumentNullException(nameof(film));
            return new ListeFilmResponseModel(film.Id, film.Ad, film.YapimYili, film.AfisDosyaAdi);
        }

        private static ListeDiziResponseModel ToDiziResponseModel(Dizi? dizi)
        {
            if (dizi == null) throw new ArgumentNullException(nameof(dizi));
            return new ListeDiziResponseModel(dizi.Id, dizi.Ad, dizi.YapimYili, dizi.AfisDosyaAdi, dizi.Durum);
        }

        public static void MapKullaniciListesiEndpoints(this IEndpointRouteBuilder app)
        {
            var grup = app.MapGroup("/api/kullanici-listeleri").WithTags("Kullanıcı Listesi İşlemleri");

            // GET /api/kullanici-listeleri/kullanici/{kullaniciId} - Bir kullanıcının tüm listelerini getir
            grup.MapGet("/kullanici/{kullaniciId:int}", async (int kullaniciId, IKullaniciListesiService listeService) =>
            {
                try
                {
                    var listeler = await listeService.GetKullaniciListeleriAsync(kullaniciId);
                    var response = listeler.Select(ToListeResponseModel).ToList();
                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Listeler getirilirken hata oluştu: {ex.Message}");
                }
            });

            // GET /api/kullanici-listeleri/{listeId} - Belirli bir listeyi getir
            grup.MapGet("/{listeId:int}", async (int listeId, IKullaniciListesiService listeService) =>
            {
                try
                {
                    var liste = await listeService.GetKullaniciListesiByIdAsync(listeId);
                    if (liste == null) return Results.NotFound(new CommonApiErrorResponseModel("Liste bulunamadı."));

                    var response = ToListeResponseModel(liste);
                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Liste getirilirken hata oluştu: {ex.Message}");
                }
            });

            // POST /api/kullanici-listeleri/kullanici/{kullaniciId} - Bir kullanıcı için yeni liste oluştur
            grup.MapPost("/kullanici/{kullaniciId:int}", async (int kullaniciId, KullaniciListesiEkleModel model, IKullaniciListesiService listeService) =>
            {
                // Validation
                if (string.IsNullOrWhiteSpace(model.ListeAdi))
                    return Results.BadRequest(new CommonApiErrorResponseModel("Liste adı boş olamaz."));

                if (kullaniciId <= 0)
                    return Results.BadRequest(new CommonApiErrorResponseModel("Geçersiz kullanıcı ID."));

                try
                {
                    var yeniListe = new KullaniciListesi { ListeAdi = model.ListeAdi.Trim(), Aciklama = model.Aciklama?.Trim() };
                    var olusturulanListe = await listeService.CreateKullaniciListesiAsync(kullaniciId, yeniListe);
                    var response = ToListeResponseModel(olusturulanListe);
                    return Results.Created($"/api/kullanici-listeleri/{olusturulanListe.Id}", response);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.Conflict(new CommonApiErrorResponseModel(ex.Message));
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new CommonApiErrorResponseModel($"Liste oluşturulurken bir hata oluştu: {ex.Message}"));
                }
            });

            // PUT /api/kullanici-listeleri/{listeId} - Bir listeyi güncelle
            grup.MapPut("/{listeId:int}", async (int listeId, KullaniciListesiGuncelleModel model, IKullaniciListesiService listeService) =>
            {
                // Validation
                if (listeId != model.Id)
                    return Results.BadRequest(new CommonApiErrorResponseModel("URL'deki liste ID ile modeldeki ID uyuşmuyor."));

                if (string.IsNullOrWhiteSpace(model.ListeAdi))
                    return Results.BadRequest(new CommonApiErrorResponseModel("Liste adı boş olamaz."));

                if (model.KullaniciId <= 0)
                    return Results.BadRequest(new CommonApiErrorResponseModel("Geçersiz kullanıcı ID."));

                try
                {
                    var mevcutListe = await listeService.GetKullaniciListesiByIdAsync(listeId);
                    if (mevcutListe == null)
                        return Results.NotFound(new CommonApiErrorResponseModel("Güncellenecek liste bulunamadı."));

                    // Ownership kontrolü
                    if (mevcutListe.KullaniciId != model.KullaniciId)
                        return Results.Forbid();

                    mevcutListe.ListeAdi = model.ListeAdi.Trim();
                    mevcutListe.Aciklama = model.Aciklama?.Trim();

                    await listeService.UpdateKullaniciListesiAsync(mevcutListe);
                    var response = ToListeResponseModel(mevcutListe);
                    return Results.Ok(response);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.Conflict(new CommonApiErrorResponseModel(ex.Message));
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new CommonApiErrorResponseModel($"Liste güncellenirken bir hata oluştu: {ex.Message}"));
                }
            });

            // DELETE /api/kullanici-listeleri/{listeId}/kullanici/{kullaniciId} - Bir listeyi sil (ownership kontrolü ile)
            grup.MapDelete("/{listeId:int}/kullanici/{kullaniciId:int}", async (int listeId, int kullaniciId, IKullaniciListesiService listeService) =>
            {
                if (kullaniciId <= 0)
                    return Results.BadRequest(new CommonApiErrorResponseModel("Geçersiz kullanıcı ID."));

                try
                {
                    var liste = await listeService.GetKullaniciListesiByIdAsync(listeId);
                    if (liste == null)
                        return Results.NotFound(new CommonApiErrorResponseModel("Silinecek liste bulunamadı."));

                    // Ownership kontrolü
                    if (liste.KullaniciId != kullaniciId)
                        return Results.Forbid();

                    await listeService.DeleteKullaniciListesiAsync(listeId);
                    return Results.Ok(new CommonApiResponseModel("Liste başarıyla silindi."));
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Liste silinirken hata oluştu: {ex.Message}");
                }
            });

            // POST /api/kullanici-listeleri/{listeId}/filmler/kullanici/{kullaniciId} - Bir listeye film ekle (ownership kontrolü ile)
            grup.MapPost("/{listeId:int}/filmler/kullanici/{kullaniciId:int}", async (int listeId, int kullaniciId, ListeIcerikModel model, IKullaniciListesiService listeService) =>
            {
                if (model.IcerikId <= 0)
                    return Results.BadRequest(new CommonApiErrorResponseModel("Geçersiz film ID."));

                if (kullaniciId <= 0)
                    return Results.BadRequest(new CommonApiErrorResponseModel("Geçersiz kullanıcı ID."));

                try
                {
                    var liste = await listeService.GetKullaniciListesiByIdAsync(listeId);
                    if (liste == null)
                        return Results.NotFound(new CommonApiErrorResponseModel("Liste bulunamadı."));

                    // Ownership kontrolü
                    if (liste.KullaniciId != kullaniciId)
                        return Results.Forbid();

                    var sonuc = await listeService.AddFilmToListesiAsync(listeId, model.IcerikId);
                    return sonuc ? Results.Ok(new CommonApiResponseModel("Film listeye eklendi.")) :
                                  Results.BadRequest(new CommonApiErrorResponseModel("Film listeye eklenemedi. Film bulunamadı ya da zaten listede."));
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Film listeye eklenirken hata oluştu: {ex.Message}");
                }
            });

            // DELETE /api/kullanici-listeleri/{listeId}/filmler/{filmId}/kullanici/{kullaniciId} - Bir listeden film çıkar
            grup.MapDelete("/{listeId:int}/filmler/{filmId:int}/kullanici/{kullaniciId:int}", async (int listeId, int filmId, int kullaniciId, IKullaniciListesiService listeService) =>
            {
                if (kullaniciId <= 0)
                    return Results.BadRequest(new CommonApiErrorResponseModel("Geçersiz kullanıcı ID."));

                try
                {
                    var liste = await listeService.GetKullaniciListesiByIdAsync(listeId);
                    if (liste == null)
                        return Results.NotFound(new CommonApiErrorResponseModel("Liste bulunamadı."));

                    // Ownership kontrolü
                    if (liste.KullaniciId != kullaniciId)
                        return Results.Forbid();

                    var sonuc = await listeService.RemoveFilmFromListesiAsync(listeId, filmId);
                    return sonuc ? Results.Ok(new CommonApiResponseModel("Film listeden çıkarıldı.")) :
                                  Results.BadRequest(new CommonApiErrorResponseModel("Film listeden çıkarılamadı. Film listede değil."));
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Film listeden çıkarılırken hata oluştu: {ex.Message}");
                }
            });

            // POST /api/kullanici-listeleri/{listeId}/diziler/kullanici/{kullaniciId} - Bir listeye dizi ekle
            grup.MapPost("/{listeId:int}/diziler/kullanici/{kullaniciId:int}", async (int listeId, int kullaniciId, ListeIcerikModel model, IKullaniciListesiService listeService) =>
            {
                if (model.IcerikId <= 0)
                    return Results.BadRequest(new CommonApiErrorResponseModel("Geçersiz dizi ID."));

                if (kullaniciId <= 0)
                    return Results.BadRequest(new CommonApiErrorResponseModel("Geçersiz kullanıcı ID."));

                try
                {
                    var liste = await listeService.GetKullaniciListesiByIdAsync(listeId);
                    if (liste == null)
                        return Results.NotFound(new CommonApiErrorResponseModel("Liste bulunamadı."));

                    // Ownership kontrolü
                    if (liste.KullaniciId != kullaniciId)
                        return Results.Forbid();

                    var sonuc = await listeService.AddDiziToListesiAsync(listeId, model.IcerikId);
                    return sonuc ? Results.Ok(new CommonApiResponseModel("Dizi listeye eklendi.")) :
                                  Results.BadRequest(new CommonApiErrorResponseModel("Dizi listeye eklenemedi. Dizi bulunamadı ya da zaten listede."));
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Dizi listeye eklenirken hata oluştu: {ex.Message}");
                }
            });

            // DELETE /api/kullanici-listeleri/{listeId}/diziler/{diziId}/kullanici/{kullaniciId} - Bir listeden dizi çıkar
            grup.MapDelete("/{listeId:int}/diziler/{diziId:int}/kullanici/{kullaniciId:int}", async (int listeId, int diziId, int kullaniciId, IKullaniciListesiService listeService) =>
            {
                if (kullaniciId <= 0)
                    return Results.BadRequest(new CommonApiErrorResponseModel("Geçersiz kullanıcı ID."));

                try
                {
                    var liste = await listeService.GetKullaniciListesiByIdAsync(listeId);
                    if (liste == null)
                        return Results.NotFound(new CommonApiErrorResponseModel("Liste bulunamadı."));

                    // Ownership kontrolü
                    if (liste.KullaniciId != kullaniciId)
                        return Results.Forbid();

                    var sonuc = await listeService.RemoveDiziFromListesiAsync(listeId, diziId);
                    return sonuc ? Results.Ok(new CommonApiResponseModel("Dizi listeden çıkarıldı.")) :
                                  Results.BadRequest(new CommonApiErrorResponseModel("Dizi listeden çıkarılamadı. Dizi listede değil."));
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Dizi listeden çıkarılırken hata oluştu: {ex.Message}");
                }
            });

            // GET /api/kullanici-listeleri/{listeId}/filmler - Bir listedeki filmleri getir
            grup.MapGet("/{listeId:int}/filmler", async (int listeId, IKullaniciListesiService listeService) =>
            {
                try
                {
                    var filmler = await listeService.GetFilmlerByListeIdAsync(listeId);
                    var response = filmler.Select(ToFilmResponseModel).ToList();
                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Filmler getirilirken hata oluştu: {ex.Message}");
                }
            });

            // GET /api/kullanici-listeleri/{listeId}/diziler - Bir listedeki dizileri getir
            grup.MapGet("/{listeId:int}/diziler", async (int listeId, IKullaniciListesiService listeService) =>
            {
                try
                {
                    var diziler = await listeService.GetDizilerByListeIdAsync(listeId);
                    var response = diziler.Select(ToDiziResponseModel).ToList();
                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Diziler getirilirken hata oluştu: {ex.Message}");
                }
            });
        }
    }
}