namespace DiziFilmTanitim.MAUI.Services
{
    public interface IUserService
    {
        bool IsLoggedIn { get; }
        int? CurrentUserId { get; }
        string? CurrentUserName { get; }

        Task SetUserSessionAsync(int userId, string userName);
        void SetCurrentUser(int userId, string userName);
        void ClearCurrentUser();
    }
}