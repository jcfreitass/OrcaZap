using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using core;
using core.Common.Notificacao.NotificacoesAgrupador;
using core.Configuration;
using core.DI;
using core.Dominio.Enums;
using core.Dominio.Quotes;
using core.Dominio.Quotes.DTO;
using core.Dominio.Retorno;
using core.Infrastructure.Repositorios.MySql;
using core.Log;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using System.Net;

[assembly: LambdaSerializer(typeof(SourceGeneratorLambdaJsonSerializer<HttpApiJsonSerializerContext>))]

namespace orcazap.quote.criar
{
    public class Function : FunctionBase
    {
        private IQuoteRepositorio _quoteRepositorio;
        private ICustomerRepositorio _customerRepositorio;
        private ILogger _log;
        private NotificacoesAgrupador _notificacoes;
        private readonly bool _isUnitTest;

        [ExcludeFromCodeCoverage]
        public Function() { }

        public Function(IQuoteRepositorio quoteRepositorio, ICustomerRepositorio customerRepositorio, ILogger log, bool isUnitTest)
        {
            _quoteRepositorio = quoteRepositorio;
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
                var dto = JsonConvert.DeserializeObject<CreateQuoteDto>(req.Body ?? string.Empty);
                if (InvalidRequest(dto, _notificacoes, _log) ||
                    dto.CustomerId <= 0 ||
                    dto.Items is null || dto.Items.Count == 0)
                {
                    _notificacoes.AddNotificacao("VALIDACAO", "CustomerId e pelo menos um item são obrigatórios.");
                    return CreateResponse(HttpStatusCode.BadRequest, new RetornoBase(HttpStatusCode.BadRequest, _notificacoes.Notificacoes));
                }

                if (_customerRepositorio.ObterPorId(dto.CustomerId, IdUsuario) is null)
                {
                    _notificacoes.AddNotificacao("CLIENTE", $"Cliente {dto.CustomerId} não encontrado.");
                    return CreateResponse(HttpStatusCode.NotFound, new RetornoBase(HttpStatusCode.NotFound, _notificacoes.Notificacoes));
                }

                var items = dto.Items.Select(i => new QuoteItem
                {
                    ServiceId = i.ServiceId,
                    Description = i.Description,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Total = i.Quantity * i.UnitPrice
                }).ToList();

                var quote = new Quote
                {
                    UserId = IdUsuario,
                    CustomerId = dto.CustomerId,
                    Status = EQuoteStatus.Draft,
                    ValidUntil = dto.ValidUntil,
                    CreatedAt = DateTime.UtcNow,
                    Total = items.Sum(i => i.Total),
                    Items = items
                };

                quote.Id = _quoteRepositorio.Inserir(quote);
                var salvo = _quoteRepositorio.ObterPorId(quote.Id, IdUsuario);

                return CreateResponse(HttpStatusCode.Created, new RetornoBase(HttpStatusCode.Created, _notificacoes.Notificacoes, QuoteDto.DePara(salvo)));
            }
            catch (Exception ex)
            {
                return LogErrorReturnInternalServerError(ex, _log, _notificacoes, "criar orçamento", lambdaContext);
            }
        }

        [ExcludeFromCodeCoverage]
        protected override void GetRepositories()
        {
            if (!_isUnitTest)
            {
                _quoteRepositorio = ResolvedorDependencia.ObterServico<IQuoteRepositorio>();
                _customerRepositorio = ResolvedorDependencia.ObterServico<ICustomerRepositorio>();
                _log = ResolvedorDependencia.ObterServico<ILogger>();
            }
            _notificacoes = ResolvedorDependencia.ObterServico<NotificacoesAgrupador>();
        }
    }
}
