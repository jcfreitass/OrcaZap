using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using core;
using core.Common.Notificacao.NotificacoesAgrupador;
using core.Configuration;
using core.DI;
using core.Dominio.Retorno;
using core.Dominio.Services.DTO;
using core.Infrastructure.Repositorios.MySql;
using core.Log;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using System.Net;

[assembly: LambdaSerializer(typeof(SourceGeneratorLambdaJsonSerializer<HttpApiJsonSerializerContext>))]

namespace orcazap.service.atualizar
{
    public class Function : FunctionBase
    {
        private IServiceRepositorio _serviceRepositorio;
        private ILogger _log;
        private NotificacoesAgrupador _notificacoes;
        private readonly bool _isUnitTest;

        [ExcludeFromCodeCoverage]
        public Function() { }

        public Function(IServiceRepositorio serviceRepositorio, ILogger log, bool isUnitTest)
        {
            _serviceRepositorio = serviceRepositorio;
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
                    _notificacoes.AddNotificacao("VALIDACAO", "ID do serviço inválido.");
                    return CreateResponse(HttpStatusCode.BadRequest, new RetornoBase(HttpStatusCode.BadRequest, _notificacoes.Notificacoes));
                }

                var dto = JsonConvert.DeserializeObject<UpdateServiceDto>(req.Body ?? string.Empty);
                if (InvalidRequest(dto, _notificacoes, _log) || string.IsNullOrWhiteSpace(dto.Name))
                {
                    _notificacoes.AddNotificacao("VALIDACAO", "Nome é obrigatório.");
                    return CreateResponse(HttpStatusCode.BadRequest, new RetornoBase(HttpStatusCode.BadRequest, _notificacoes.Notificacoes));
                }

                var service = _serviceRepositorio.ObterPorId(id, IdUsuario);
                if (service is null)
                {
                    _notificacoes.AddNotificacao("SERVICO", $"Serviço {id} não encontrado.");
                    return CreateResponse(HttpStatusCode.NotFound, new RetornoBase(HttpStatusCode.NotFound, _notificacoes.Notificacoes));
                }

                service.Name = dto.Name;
                service.Description = dto.Description;
                service.Price = dto.Price;
                _serviceRepositorio.Atualizar(service);

                return CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                return LogErrorReturnInternalServerError(ex, _log, _notificacoes, "atualizar serviço", lambdaContext);
            }
        }

        [ExcludeFromCodeCoverage]
        protected override void GetRepositories()
        {
            if (!_isUnitTest)
            {
                _serviceRepositorio = ResolvedorDependencia.ObterServico<IServiceRepositorio>();
                _log = ResolvedorDependencia.ObterServico<ILogger>();
            }
            _notificacoes = ResolvedorDependencia.ObterServico<NotificacoesAgrupador>();
        }
    }
}
