namespace core.Configuration
{
    public class JwtSettings
    {
        private const string CHAVE = "jwt_secret";

        public string Secret()
        {
            return ConfigurationManager.GetAppSetting(CHAVE)
                ?? throw new ArgumentException($"Configuração '{CHAVE}' não encontrada.");
        }
    }
}
