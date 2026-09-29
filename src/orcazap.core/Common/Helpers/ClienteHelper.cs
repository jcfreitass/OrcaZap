using Amazon.Lambda.APIGatewayEvents;
using core.Common.Notificacao.NotificacoesAgrupador;
using core.DI;
using core.Security;

namespace core.Common.Helpers
{
    public abstract class ClienteHelper
    {
        protected static long IdUsuario { get; private set; }
        protected static string EmailUsuario { get; private set; }
        protected static string EnderecoIP { get; private set; }

        protected static void HidratarContextoUsuario(APIGatewayHttpApiV2ProxyRequest request)
        {
            IdUsuario = 0;
            EmailUsuario = null;
            EnderecoIP = request?.RequestContext?.Http?.SourceIp;
        }

        protected static void HidratarContextoUsuario(APIGatewayProxyRequest request)
        {
            IdUsuario = 0;
            EmailUsuario = null;
            EnderecoIP = request?.RequestContext?.Identity?.SourceIp;
        }

        protected static bool AutenticacaoInvalida(APIGatewayHttpApiV2ProxyRequest request, NotificacoesAgrupador notificacoes)
        {
            var token = ExtrairTokenBearer(request?.Headers);
            return AutenticacaoInvalida(token, notificacoes);
        }

        protected static bool AutenticacaoInvalida(APIGatewayProxyRequest request, NotificacoesAgrupador notificacoes)
        {
            var token = ExtrairTokenBearer(request?.Headers);
            return AutenticacaoInvalida(token, notificacoes);
        }

        private static bool AutenticacaoInvalida(string token, NotificacoesAgrupador notificacoes)
        {
            var usuario = string.IsNullOrWhiteSpace(token)
                ? null
                : ResolvedorDependencia.ObterServico<IJwtService>().ValidarToken(token);

            if (usuario is null)
            {
                notificacoes.AddNotificacao("AUTENTICACAO", "Token de autenticação ausente ou inválido.");
                return true;
            }

            IdUsuario = usuario.IdUsuario;
            EmailUsuario = usuario.Email;
            return false;
        }

        private static string ExtrairTokenBearer(IDictionary<string, string> headers)
        {
            if (headers is null) return null;

            var authorization = headers
                .Where(h => string.Equals(h.Key, "Authorization", StringComparison.OrdinalIgnoreCase))
                .Select(h => h.Value)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(authorization)) return null;

            const string prefixo = "Bearer ";
            return authorization.StartsWith(prefixo, StringComparison.OrdinalIgnoreCase)
                ? authorization[prefixo.Length..]
                : null;
        }
    }
}
