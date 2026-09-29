using Microsoft.Extensions.Configuration;

namespace core.Configuration
{
    public static class ConfigurationManager
    {
        public static IConfigurationRoot Configuration { get; private set; }

        static ConfigurationManager()
        {
            BuildConfiguration();
        }

        public static void ReloadConfiguration()
        {
            BuildConfiguration();
        }

        public static string GetAppSetting(string key)
        {
            return Configuration[key];
        }

        private static void BuildConfiguration()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "dev";

            Configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
