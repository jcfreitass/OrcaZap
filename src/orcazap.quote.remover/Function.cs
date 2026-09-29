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

namespace orcazap.quote.remover
{
    public class Function : FunctionBase
    {
        private IQuoteRepositorio _quoteRepositorio;
        private ILogger _log;
        private NotificacoesAgrupador _notificacoes;
        private readonly bool _isUnitTest;

        [ExcludeFromCodeCoverage]
        public Function() { }

        public Function(IQuoteRepositorio quoteRepositorio, ILogger log, bool isUnitTest)
        {
            _quoteRepositorio = quoteRepositorio;
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
                    _notificacoes.AddNotificacao("VALIDACAO", "ID do orçamento inválido.");
                    return CreateResponse(HttpStatusCode.BadRequest, new RetornoBase(HttpStatusCode.BadRequest, _notificacoes.Notificacoes));
                }

                if (!_quoteRepositorio.Remover(id, IdUsuario))
                {
                    _notificacoes.AddNotificacao("ORCAMENTO", $"Orçamento {id} não encontrado.");
                    return CreateResponse(HttpStatusCode.NotFound, new RetornoBase(HttpStatusCode.NotFound, _notificacoes.Notificacoes));
                }

                return CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                return LogErrorReturnInternalServerError(ex, _log, _notificacoes, "remover orçamento", lambdaContext);
            }
        }

        [ExcludeFromCodeCoverage]
        protected override void GetRepositories()
        {
            if (!_isUnitTest)
            {
                _quoteRepositorio = ResolvedorDependencia.ObterServico<IQuoteRepositorio>();
                _log = ResolvedorDependencia.ObterServico<ILogger>();
            }
            _notificacoes = ResolvedorDependencia.ObterServico<NotificacoesAgrupador>();
        }
    }
}
