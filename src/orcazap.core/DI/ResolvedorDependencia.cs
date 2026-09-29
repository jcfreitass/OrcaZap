using core.Common.Notificacao;
using core.Common.Notificacao.NotificacoesAgrupador;
using core.Configuration;
using core.Infrastructure.Data.MySql;
using core.Infrastructure.Repositorios.MySql;
using core.Log;
using core.Security;
using Microsoft.Extensions.DependencyInjection;

namespace core.DI
{
    public static class ResolvedorDependencia
    {
        public static IServiceProvider provedorServico { get; private set; }

        static ResolvedorDependencia()
        {
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<Func<int, MySqlDbConnection>>(servidor =>
            {
                return new MySqlDbConnection(servidor);
            });

            AdicionarVinculos(serviceCollection);
            AdicionarVinculosLogging(serviceCollection);
            AdicionarVinculosSeguranca(serviceCollection);
            ConfigurarSettings(serviceCollection);

            provedorServico = serviceCollection.BuildServiceProvider();
        }

        public static T ObterServico<T>()
        {
            try
            {
                return provedorServico.GetService<T>();
            }
            catch (Exception exception)
            {
                throw new Exception("Erro ao resolver dependência.", exception);
            }
        }

        private static void AdicionarVinculos(IServiceCollection servicos)
        {
            servicos.AddTransient<IUserRepositorio, UserRepositorio>();
            servicos.AddTransient<ICustomerRepositorio, CustomerRepositorio>();
            servicos.AddTransient<IServiceRepositorio, ServiceRepositorio>();
            servicos.AddTransient<IQuoteRepositorio, QuoteRepositorio>();

            servicos.AddTransient<NotificacoesAgrupador>();
            servicos.AddTransient<NotificacaoAgrupador>();
        }

        private static void AdicionarVinculosLogging(IServiceCollection servicos)
        {
            servicos.AddTransient<ILogger, Logger>();
        }

        private static void AdicionarVinculosSeguranca(IServiceCollection servicos)
        {
            servicos.AddSingleton<JwtSettings>();
            servicos.AddSingleton<IJwtService, JwtService>();
            servicos.AddSingleton<IPasswordHasher, PasswordHasher>();
        }

        public static void ConfigurarSettings(IServiceCollection servicos)
        {
            servicos.AddOptions();
            servicos.Configure<LogLevelSettings>(ConfigurationManager.Configuration.GetSection("logging_level"));
        }
    }
}
