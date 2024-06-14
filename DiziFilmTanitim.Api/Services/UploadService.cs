using DiziFilmTanitim.Api.Interfaces;
using DiziFilmTanitim.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace DiziFilmTanitim.Api.Services
{
    public class UploadService : IUploadService
    {
        private readonly string _wwwrootPath;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB
        private readonly IFilmService _filmService;
        private readonly IDiziService _diziService;
        private readonly IOyuncuService _oyuncuService;
        private readonly IYonetmenService _yonetmenService;

        public UploadService(IWebHostEnvironment environment,
                           IFilmService filmService,
                           IDiziService diziService,
                           IOyuncuService oyuncuService,
                           IYonetmenService yonetmenService)
        {
            // API katmanındaki wwwroot klasörünü kullan
            _wwwrootPath = environment.WebRootPath;
            _filmService = filmService;
            _diziService = diziService;
            _oyuncuService = oyuncuService;
            _yonetmenService = yonetmenService;
        }

        public async Task<(bool success, string fileName, string message)> UploadFilmAfisAsync(int filmId, IFormFile file)
        {
            var result = await UploadFileAsync(file, "afisler", "film", filmId);

            if (result.success)
            {
                // Database'de film kaydını güncelle
                await _filmService.UpdateFilmAfisAsync(filmId, result.fileName);
            }

            return result;
        }

        public async Task<(bool success, string fileName, string message)> UploadDiziAfisAsync(int diziId, IFormFile file)
        {
            var result = await UploadFileAsync(file, "afisler", "dizi", diziId);

            if (result.success)
            {
                // Database'de dizi kaydını güncelle
                await _diziService.UpdateDiziAfisAsync(diziId, result.fileName);
            }

            return result;
        }

        public async Task<(bool success, string fileName, string message)> UploadOyuncuFotografAsync(int oyuncuId, IFormFile file)
        {
            var result = await UploadFileAsync(file, "fotograflar", "oyuncu", oyuncuId);

            if (result.success)
            {
                // Database'de oyuncu kaydını güncelle
                await _oyuncuService.UpdateOyuncuFotografAsync(oyuncuId, result.fileName);
            }

            return result;
        }

        public async Task<(bool success, string fileName, string message)> UploadYonetmenFotografAsync(int yonetmenId, IFormFile file)
        {
            var result = await UploadFileAsync(file, "fotograflar", "yonetmen", yonetmenId);

            if (result.success)
            {
                // Database'de yönetmen kaydını güncelle
                await _yonetmenService.UpdateYonetmenFotografAsync(yonetmenId, result.fileName);
            }

            return result;
        }

        private async Task<(bool success, string fileName, string message)> UploadFileAsync(IFormFile file, string category, string entityType, int entityId)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return (false, "", "Dosya seçilmedi.");

                // Dosya türü kontrolü
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (!_allowedExtensions.Contains(extension))
                    return (false, "", "Sadece jpg, jpeg, png ve webp dosyaları kabul edilir.");

                // Dosya boyutu kontrolü
                if (file.Length > MaxFileSizeBytes)
                    return (false, "", "Dosya boyutu 5MB'dan büyük olamaz.");

                // Dosya adı oluştur
                var fileName = $"{entityType}_{entityId}_{DateTime.Now:yyyyMMdd_HHmmss}{extension}";
                var categoryPath = Path.Combine(_wwwrootPath, "uploads", category);
                var filePath = Path.Combine(categoryPath, fileName);

                // Klasör yoksa oluştur
                Directory.CreateDirectory(categoryPath);

                // Dosyayı kaydet
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return (true, fileName, $"{entityType} fotoğrafı başarıyla yüklendi.");
            }
            catch (Exception ex)
            {
                return (false, "", $"Dosya yüklenirken hata oluştu: {ex.Message}");
            }
        }

        public Task<bool> DeleteFileAsync(string fileName, string category)
        {
            try
            {
                var filePath = Path.Combine(_wwwrootPath, "uploads", category, fileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public string GetFileUrl(string fileName, string category)
        {
            return $"/uploads/{category}/{fileName}";
        }
    }
}