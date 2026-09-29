using Microsoft.Extensions.Options;

namespace core.Log
{
    public class Logger : ILogger
    {
        private readonly string _minimumLevel;

        public Logger(IOptions<LogLevelSettings> settings)
        {
            _minimumLevel = settings?.Value?.MinimumLevel ?? "Information";
        }

        public void LogInfo(string message)
        {
            Console.WriteLine($"[INFO] {DateTime.UtcNow:O} {message}");
        }

        public void LogError(string message)
        {
            Console.Error.WriteLine($"[ERROR] {DateTime.UtcNow:O} {message}");
        }

        public void LogDebug(string message)
        {
            if (string.Equals(_minimumLevel, "Debug", StringComparison.OrdinalIgnoreCase))
                Console.WriteLine($"[DEBUG] {DateTime.UtcNow:O} {message}");
        }
    }
}
