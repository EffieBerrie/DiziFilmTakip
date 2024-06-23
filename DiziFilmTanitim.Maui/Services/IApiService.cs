namespace DiziFilmTanitim.MAUI.Services
{
    public interface IApiService
    {
        // HttpClient base işlemleri için temel interface
        Task<T?> GetAsync<T>(string endpoint);
        Task<T?> PostAsync<T>(string endpoint, object data);
        Task<T?> PutAsync<T>(string endpoint, object data);
        Task<bool> DeleteAsync(string endpoint);

        // Authentication
        void SetAuthToken(string token);
        void ClearAuthToken();

        // Puanlama metodları
        Task<bool> PuanlaAsync(int filmId, int kullaniciId, int puan);
        Task<double?> OrtalamaPuanGetirAsync(int filmId);
        Task<int?> KullaniciPuanGetirAsync(int filmId, int kullaniciId);

        // Dizi Puanlama Metodları
        Task<bool> PuanlaDiziAsync(int diziId, int kullaniciId, int puan);
        Task<double?> OrtalamaDiziPuaniGetirAsync(int diziId);
        Task<int?> KullaniciDiziPuaniGetirAsync(int diziId, int kullaniciId);

        // Kullanıcı Listesi Metodları
        Task<T?> CreateKullaniciListesiAsync<T>(int kullaniciId, object listeData);
        Task<bool> AddFilmToListeAsync(int listeId, int kullaniciId, int filmId);
        Task<bool> AddDiziToListeAsync(int listeId, int kullaniciId, int diziId);
        Task<bool> RemoveFilmFromListeAsync(int listeId, int kullaniciId, int filmId);
        Task<bool> RemoveDiziFromListeAsync(int listeId, int kullaniciId, int diziId);
    }
}