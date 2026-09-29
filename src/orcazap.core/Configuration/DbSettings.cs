namespace core.Configuration
{
    public class DbSettings
    {
        private readonly string _key;

        public DbSettings(string key)
        {
            _key = key;
        }

        public string ConnectionString()
        {
            return ConfigurationManager.GetAppSetting(_key)
                ?? throw new ArgumentException($"Connection string não configurada para a chave '{_key}'.");
        }
    }
}
