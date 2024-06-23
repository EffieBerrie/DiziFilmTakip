using Microsoft.Extensions.Logging;

namespace DiziFilmTanitim.MAUI.Services
{
    public interface ILoggingService
    {
        void LogDebug(string message);
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message, Exception? exception = null);
    }

    public class LoggingService : ILoggingService
    {
        private readonly ILogger<LoggingService> _logger;

        public LoggingService(ILogger<LoggingService> logger)
        {
            _logger = logger;
        }

        public void LogDebug(string message)
        {
#if DEBUG
            _logger.LogDebug(message);
            System.Diagnostics.Debug.WriteLine($"DEBUG: {message}");
#endif
        }

        public void LogInfo(string message)
        {
            _logger.LogInformation(message);
        }

        public void LogWarning(string message)
        {
            _logger.LogWarning(message);
        }

        public void LogError(string message, Exception? exception = null)
        {
            _logger.LogError(exception, message);
        }
    }
}