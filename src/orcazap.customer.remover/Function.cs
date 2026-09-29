using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using core;
using core.Common.Notificacao.NotificacoesAgrupador;
using core.Configuration;
using core.DI;
using core.Dominio.Retorno;
using core.Infrastructure.Repositorios.MySql;
using core.Log;
using System.Diagnostics.CodeAnalysis;
using System.Net;

[assembly: LambdaSerializer(typeof(SourceGeneratorLambdaJsonSerializer<HttpApiJsonSerializerContext>))]

namespace orcazap.customer.remover
{
    public class Function : FunctionBase
    {
        private ICustomerRepositorio _customerRepositorio;
        private ILogger _log;
        private NotificacoesAgrupador _notificacoes;
        private readonly bool _isUnitTest;

        [ExcludeFromCodeCoverage]
        public Function() { }

        public Function(ICustomerRepositorio customerRepositorio, ILogger log, bool isUnitTest)
        {
            _customerRepositorio = customerRepositorio;
            _log = log;
            _isUnitTest = isUnitTest;
        }

        public APIGatewayHttpApiV2ProxyResponse FunctionHandler(APIGatewayHttpApiV2ProxyRequest req, ILambdaContext lambdaContext)
        {
            SetEnvironment(req);
            Startup();

            if (AutenticacaoInvalida(req, _notificacoes))
                return CreateResponse(HttpStatusCode.Unauthorized, new RetornoBase(HttpStatusCode.Unauthorized, _notificacoes.Notificacoes));

            try
            {
                if (!req.PathParameters.TryGetValue("id", out var idStr) || !long.TryParse(idStr, out var id))
                {
                    _notificacoes.AddNotificacao("VALIDACAO", "ID do cliente inválido.");
                    return CreateResponse(HttpStatusCode.BadRequest, new RetornoBase(HttpStatusCode.BadRequest, _notificacoes.Notificacoes));
                }

                if (!_customerRepositorio.Remover(id, IdUsuario))
                {
                    _notificacoes.AddNotificacao("CLIENTE", $"Cliente {id} não encontrado.");
                    return CreateResponse(HttpStatusCode.NotFound, new RetornoBase(HttpStatusCode.NotFound, _notificacoes.Notificacoes));
                }

                return CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                return LogErrorReturnInternalServerError(ex, _log, _notificacoes, "remover cliente", lambdaContext);
            }
        }

        [ExcludeFromCodeCoverage]
        protected override void GetRepositories()
        {
            if (!_isUnitTest)
            {
                _customerRepositorio = ResolvedorDependencia.ObterServico<ICustomerRepositorio>();
                _log = ResolvedorDependencia.ObterServico<ILogger>();
            }
            _notificacoes = ResolvedorDependencia.ObterServico<NotificacoesAgrupador>();
        }
    }
}
