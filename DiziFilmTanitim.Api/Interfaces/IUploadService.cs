using Microsoft.AspNetCore.Http;

namespace DiziFilmTanitim.Api.Interfaces
{
    public interface IUploadService
    {
        Task<(bool success, string fileName, string message)> UploadFilmAfisAsync(int filmId, IFormFile file);
        Task<(bool success, string fileName, string message)> UploadDiziAfisAsync(int diziId, IFormFile file);
        Task<(bool success, string fileName, string message)> UploadOyuncuFotografAsync(int oyuncuId, IFormFile file);
        Task<(bool success, string fileName, string message)> UploadYonetmenFotografAsync(int yonetmenId, IFormFile file);
        Task<bool> DeleteFileAsync(string fileName, string category);
        string GetFileUrl(string fileName, string category);
    }
}