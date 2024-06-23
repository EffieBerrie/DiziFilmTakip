using System.Text;
using System.Text.Json;
using DiziFilmTanitim.MAUI.Models;

namespace DiziFilmTanitim.MAUI.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ILoggingService _logger;

        public ApiService(HttpClient httpClient, ILoggingService logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                _logger.LogDebug($"GET {_httpClient.BaseAddress}{endpoint}");
                var response = await _httpClient.GetAsync(endpoint);

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"Response Status: {response.StatusCode}");
                _logger.LogDebug($"Response Content: {content}");

                response.EnsureSuccessStatusCode();

                return JsonSerializer.Deserialize<T>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GET Error - {ex.Message}", ex);
                return default;
            }
        }

        public async Task<T?> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                _logger.LogDebug($"POST {_httpClient.BaseAddress}{endpoint}");
                _logger.LogDebug($"Request Body: {json}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(endpoint, content);

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"Response Status: {response.StatusCode}");
                _logger.LogDebug($"Response Content: {responseContent}");

                // Başarılı status kodlar (200-299) için
                if (response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                }
                else
                {
                    // Hata durumlarında response body'sini parse etmeye çalış
                    try
                    {
                        // API'den dönen hata mesajını almaya çalış
                        var errorResponse = JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                        return errorResponse; // Hata response'unu da döndür ki ViewModel'de işlenebilsin
                    }
                    catch
                    {
                        // Parse edilemezse default döndür
                        return default;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"POST Error - {ex.Message}", ex);
                return default;
            }
        }

        public async Task<T?> PutAsync<T>(string endpoint, object data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                _logger.LogDebug($"PUT {_httpClient.BaseAddress}{endpoint}");
                _logger.LogDebug($"Request Body: {json}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(endpoint, content);

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"Response Status: {response.StatusCode}");
                _logger.LogDebug($"Response Content: {responseContent}");

                // Başarılı status kodlar (200-299) için
                if (response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                }
                else
                {
                    // Hata durumlarında response body'sini parse etmeye çalış
                    _logger.LogWarning($"PUT Request failed with status {response.StatusCode}");
                    try
                    {
                        // API'den dönen hata mesajını almaya çalış
                        var errorResponse = JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                        _logger.LogInfo($"Parsed error response: {JsonSerializer.Serialize(errorResponse, _jsonOptions)}");
                        return errorResponse; // Hata response'unu da döndür ki ViewModel'de işlenebilsin
                    }
                    catch (Exception parseEx)
                    {
                        // Parse edilemezse default döndür
                        _logger.LogError($"Failed to parse error response: {parseEx.Message}");
                        return default;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"PUT Error - {ex.Message}", ex);
                return default;
            }
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                _logger.LogDebug($"DELETE {_httpClient.BaseAddress}{endpoint}");

                var response = await _httpClient.DeleteAsync(endpoint);

                _logger.LogDebug($"Response Status: {response.StatusCode}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError($"DELETE Error - {ex.Message}", ex);
                return false;
            }
        }

        public void SetAuthToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        public void ClearAuthToken()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        // Puanlama metodları
        public async Task<bool> PuanlaAsync(int filmId, int kullaniciId, int puan)
        {
            try
            {
                var puanlamaRequest = new FilmPuanlamaRequest { Puan = puan };
                var endpoint = $"api/filmler/{filmId}/kullanici/{kullaniciId}/puanla";

                _logger.LogDebug($"POST {_httpClient.BaseAddress}{endpoint}");
                _logger.LogDebug($"Puanlama - Film:{filmId}, Kullanıcı:{kullaniciId}, Puan:{puan}");

                var json = JsonSerializer.Serialize(puanlamaRequest, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content);

                _logger.LogDebug($"Response Status: {response.StatusCode}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Puanlama hatası: {ex.Message}", ex);
                return false;
            }
        }

        public async Task<double?> OrtalamaPuanGetirAsync(int filmId)
        {
            try
            {
                var response = await GetAsync<OrtalamaPuanResponse>($"api/filmler/{filmId}/ortalama-puan");
                return response?.OrtalamaPuan;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ortalama puan getirme hatası: {ex.Message}", ex);
                return null;
            }
        }

        public async Task<int?> KullaniciPuanGetirAsync(int filmId, int kullaniciId)
        {
            try
            {
                var response = await GetAsync<KullaniciPuanResponse>($"api/filmler/{filmId}/kullanici/{kullaniciId}/puan");
                return response?.Puan;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Kullanıcı puan getirme hatası: {ex.Message}", ex);
                return null;
            }
        }

        // Dizi Puanlama Metodları
        public async Task<bool> PuanlaDiziAsync(int diziId, int kullaniciId, int puan)
        {
            try
            {
                var puanlamaRequest = new FilmPuanlamaRequest { Puan = puan }; // Dizi için de aynı request modeli kullanılabilir.
                var endpoint = $"api/diziler/{diziId}/kullanici/{kullaniciId}/puanla";

                _logger.LogDebug($"POST {_httpClient.BaseAddress}{endpoint}");
                _logger.LogDebug($"Dizi Puanlama - Dizi:{diziId}, Kullanıcı:{kullaniciId}, Puan:{puan}");

                var json = JsonSerializer.Serialize(puanlamaRequest, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content);

                _logger.LogDebug($"Response Status: {response.StatusCode}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Dizi puanlama hatası: {ex.Message}", ex);
                return false;
            }
        }

        public async Task<double?> OrtalamaDiziPuaniGetirAsync(int diziId)
        {
            try
            {
                var response = await GetAsync<OrtalamaDiziPuanResponse>($"api/diziler/{diziId}/ortalama-puan");
                return response?.OrtalamaPuan;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Dizi ortalama puan getirme hatası: {ex.Message}", ex);
                return null;
            }
        }

        public async Task<int?> KullaniciDiziPuaniGetirAsync(int diziId, int kullaniciId)
        {
            try
            {
                var response = await GetAsync<KullaniciDiziPuanResponse>($"api/diziler/{diziId}/kullanici/{kullaniciId}/puan");
                return response?.Puan;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Kullanıcı dizi puanı getirme hatası: {ex.Message}", ex);
                return null;
            }
        }

        // Kullanıcı Listesi Metodları
        public async Task<T?> CreateKullaniciListesiAsync<T>(int kullaniciId, object listeData)
        {
            try
            {
                var endpoint = $"api/kullanici-listeleri/kullanici/{kullaniciId}";
                _logger.LogDebug($"POST {_httpClient.BaseAddress}{endpoint}");
                _logger.LogDebug($"Liste Oluşturma - Kullanıcı:{kullaniciId}, Data:{JsonSerializer.Serialize(listeData, _jsonOptions)}");

                var response = await PostAsync<T>(endpoint, listeData);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Kullanıcı listesi oluşturma hatası: {ex.Message}", ex);
                return default;
            }
        }

        public async Task<bool> AddFilmToListeAsync(int listeId, int kullaniciId, int filmId)
        {
            try
            {
                var endpoint = $"api/kullanici-listeleri/{listeId}/filmler/kullanici/{kullaniciId}";
                var data = new { IcerikId = filmId };

                _logger.LogDebug($"POST {_httpClient.BaseAddress}{endpoint}");
                _logger.LogDebug($"Film Listeye Ekleme - Liste:{listeId}, Kullanıcı:{kullaniciId}, Film:{filmId}");

                var response = await PostAsync<CommonApiResponseModel>(endpoint, data);
                return response?.Success == true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Film listeye ekleme hatası: {ex.Message}", ex);
                return false;
            }
        }

        public async Task<bool> AddDiziToListeAsync(int listeId, int kullaniciId, int diziId)
        {
            try
            {
                var endpoint = $"api/kullanici-listeleri/{listeId}/diziler/kullanici/{kullaniciId}";
                var data = new { IcerikId = diziId };

                _logger.LogDebug($"POST {_httpClient.BaseAddress}{endpoint}");
                _logger.LogDebug($"Dizi Listeye Ekleme - Liste:{listeId}, Kullanıcı:{kullaniciId}, Dizi:{diziId}");

                var response = await PostAsync<CommonApiResponseModel>(endpoint, data);
                return response?.Success == true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Dizi listeye ekleme hatası: {ex.Message}", ex);
                return false;
            }
        }

        public async Task<bool> RemoveFilmFromListeAsync(int listeId, int kullaniciId, int filmId)
        {
            try
            {
                var endpoint = $"api/kullanici-listeleri/{listeId}/filmler/{filmId}/kullanici/{kullaniciId}";

                _logger.LogDebug($"DELETE {_httpClient.BaseAddress}{endpoint}");
                _logger.LogDebug($"Film Listeden Çıkarma - Liste:{listeId}, Kullanıcı:{kullaniciId}, Film:{filmId}");

                return await DeleteAsync(endpoint);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Film listeden çıkarma hatası: {ex.Message}", ex);
                return false;
            }
        }

        public async Task<bool> RemoveDiziFromListeAsync(int listeId, int kullaniciId, int diziId)
        {
            try
            {
                var endpoint = $"api/kullanici-listeleri/{listeId}/diziler/{diziId}/kullanici/{kullaniciId}";

                _logger.LogDebug($"DELETE {_httpClient.BaseAddress}{endpoint}");
                _logger.LogDebug($"Dizi Listeden Çıkarma - Liste:{listeId}, Kullanıcı:{kullaniciId}, Dizi:{diziId}");

                return await DeleteAsync(endpoint);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Dizi listeden çıkarma hatası: {ex.Message}", ex);
                return false;
            }
        }
    }
}