using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using core;
using core.Common.Notificacao.NotificacoesAgrupador;
using core.Configuration;
using core.DI;
using core.Dominio.Customers;
using core.Dominio.Customers.DTO;
using core.Dominio.Retorno;
using core.Infrastructure.Repositorios.MySql;
using core.Log;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using System.Net;

[assembly: LambdaSerializer(typeof(SourceGeneratorLambdaJsonSerializer<HttpApiJsonSerializerContext>))]

namespace orcazap.customer.criar
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
                var dto = JsonConvert.DeserializeObject<CreateCustomerDto>(req.Body ?? string.Empty);
                if (InvalidRequest(dto, _notificacoes, _log) ||
                    string.IsNullOrWhiteSpace(dto.Name))
                {
                    _notificacoes.AddNotificacao("VALIDACAO", "Nome é obrigatório.");
                    return CreateResponse(HttpStatusCode.BadRequest, new RetornoBase(HttpStatusCode.BadRequest, _notificacoes.Notificacoes));
                }

                var customer = new Customer
                {
                    UserId = IdUsuario,
                    Name = dto.Name,
                    Phone = dto.Phone,
                    Email = dto.Email,
                    CreatedAt = DateTime.UtcNow
                };

                customer.Id = _customerRepositorio.Inserir(customer);

                return CreateResponse(HttpStatusCode.Created, new RetornoBase(HttpStatusCode.Created, _notificacoes.Notificacoes, CustomerDto.DePara(customer)));
            }
            catch (Exception ex)
            {
                return LogErrorReturnInternalServerError(ex, _log, _notificacoes, "criar cliente", lambdaContext);
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
