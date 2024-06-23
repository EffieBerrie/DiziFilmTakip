using Microsoft.Maui.Storage;
using Microsoft.Extensions.Logging;

namespace DiziFilmTanitim.MAUI.Services
{
    public class UserService : IUserService
    {
        private const string UserIdKey = "current_user_id";
        private const string UserNameKey = "current_user_name";

        private readonly ILoggingService _logger;

        public bool IsLoggedIn => CurrentUserId.HasValue;
        public int? CurrentUserId { get; private set; }
        public string? CurrentUserName { get; private set; }

        public UserService(ILoggingService logger)
        {
            _logger = logger;
            LoadUserFromSecureStorage();
        }

        private async void LoadUserFromSecureStorage()
        {
            try
            {
                var userIdStr = await SecureStorage.Default.GetAsync(UserIdKey);
                var userName = await SecureStorage.Default.GetAsync(UserNameKey);

                if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId) && !string.IsNullOrEmpty(userName))
                {
                    CurrentUserId = userId;
                    CurrentUserName = userName;
                    _logger.LogInfo($"Kullanıcı oturumu SecureStorage'dan yüklendi: {userName} (ID: {userId})");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("SecureStorage'dan kullanıcı okunurken hata oluştu.", ex);
            }
        }

        public async Task SetUserSessionAsync(int userId, string userName)
        {
            SetCurrentUser(userId, userName);
            try
            {
                await SecureStorage.Default.SetAsync(UserIdKey, userId.ToString());
                await SecureStorage.Default.SetAsync(UserNameKey, userName);
                _logger.LogInfo($"Kullanıcı oturumu SecureStorage'a kaydedildi: {userName} (ID: {userId})");
            }
            catch (Exception ex)
            {
                _logger.LogError("SecureStorage'a kullanıcı kaydedilirken hata oluştu.", ex);
            }
        }

        public void SetCurrentUser(int userId, string userName)
        {
            CurrentUserId = userId;
            CurrentUserName = userName;
        }

        public void ClearCurrentUser()
        {
            CurrentUserId = null;
            CurrentUserName = null;
            try
            {
                SecureStorage.Default.Remove(UserIdKey);
                SecureStorage.Default.Remove(UserNameKey);
                _logger.LogInfo("Kullanıcı oturumu SecureStorage'dan temizlendi.");
            }
            catch (Exception ex)
            {
                _logger.LogError("SecureStorage'dan kullanıcı temizlenirken hata oluştu.", ex);
            }
        }
    }
}