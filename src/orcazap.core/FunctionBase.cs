using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using core.Common.Helpers;
using core.Common.Notificacao;
using core.Common.Notificacao.NotificacoesAgrupador;
using core.Configuration;
using core.Dominio.Retorno;
using core.Log;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace core
{
    public abstract class FunctionBase : ClienteHelper
    {
        private const string DEV_ENVIRONMENT = "dev";
        private const string HML_ENVIRONMENT = "hml";
        private const string PRD_ENVIRONMENT = "prd";

        protected FunctionBase() { }

        protected void Startup()
        {
            GetRepositories();
        }

        protected abstract void GetRepositories();

        protected static void SetEnvironment(APIGatewayHttpApiV2ProxyRequest request)
        {
            ConfigurationManager.ReloadConfiguration();

            var stageAlias = string.Empty;
            if (request?.StageVariables != null && request.StageVariables.ContainsKey(nameof(stageAlias)))
                stageAlias = request.StageVariables[nameof(stageAlias)];

            if (!string.IsNullOrWhiteSpace(stageAlias) &&
                (stageAlias == PRD_ENVIRONMENT || stageAlias == HML_ENVIRONMENT || stageAlias == DEV_ENVIRONMENT))
                Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", stageAlias);
            else
                Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", DEV_ENVIRONMENT);

            HidratarContextoUsuario(request);
        }

        protected static void SetEnvironment(APIGatewayProxyRequest request)
        {
            ConfigurationManager.ReloadConfiguration();

            var stageAlias = string.Empty;
            if (request?.StageVariables != null && request.StageVariables.ContainsKey(nameof(stageAlias)))
                stageAlias = request.StageVariables[nameof(stageAlias)];

            if (!string.IsNullOrWhiteSpace(stageAlias) &&
                (stageAlias == PRD_ENVIRONMENT || stageAlias == HML_ENVIRONMENT || stageAlias == DEV_ENVIRONMENT))
                Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", stageAlias);
            else
                Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", DEV_ENVIRONMENT);

            HidratarContextoUsuario(request);
        }

        protected static APIGatewayHttpApiV2ProxyResponse CreateResponse(HttpStatusCode statusCode, object result = null)
        {
            var body = result != null ? JsonConvert.SerializeObject(result) : null;

            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int)statusCode,
                Body = body,
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Headers", "*" }
                }
            };
        }

        protected static APIGatewayProxyResponse CreateResponseV1(object result)
        {
            var statusCode = result != null ? (int)HttpStatusCode.OK : (int)HttpStatusCode.InternalServerError;
            var body = result != null ? JsonConvert.SerializeObject(result) : null;

            return new APIGatewayProxyResponse
            {
                StatusCode = statusCode,
                Body = body,
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Headers", "*" }
                }
            };
        }

        protected static bool InvalidRequest<T>(T dto, NotificacoesAgrupador notificacoes, ILogger log) where T : class
        {
            if (dto is null)
            {
                notificacoes.AddNotificacao("VALIDACAO", "Corpo da requisição é obrigatório.");
                log.LogError("Requisição sem corpo.");
                return true;
            }
            return false;
        }

        [ExcludeFromCodeCoverage]
        protected static APIGatewayHttpApiV2ProxyResponse LogErrorReturnInternalServerError(
            Exception ex,
            ILogger log,
            NotificacoesAgrupador notificacoes,
            string acao,
            ILambdaContext lambdaContext)
        {
            log.LogError($"Erro ao {acao} - IdUsuario: {IdUsuario} - Email: {EmailUsuario} - IP: {EnderecoIP} - Env: {lambdaContext?.InvokedFunctionArn} - Message: {ex.Message} - StackTrace: {ex.StackTrace}");
            notificacoes.AddNotificacao("Erro", "Ocorreu um erro ao tentar processar sua solicitação. Tente novamente em instantes.");
            return CreateResponse(HttpStatusCode.InternalServerError, new RetornoBase(HttpStatusCode.InternalServerError, notificacoes.Notificacoes));
        }
    }
}
