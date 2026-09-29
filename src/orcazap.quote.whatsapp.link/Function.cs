using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using core;
using core.Common.Notificacao.NotificacoesAgrupador;
using core.Configuration;
using core.DI;
using core.Dominio.Quotes;
using core.Dominio.Quotes.DTO;
using core.Dominio.Retorno;
using core.Infrastructure.Repositorios.MySql;
using core.Log;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;

[assembly: LambdaSerializer(typeof(SourceGeneratorLambdaJsonSerializer<HttpApiJsonSerializerContext>))]

namespace orcazap.quote.whatsapp.link
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

                var quote = _quoteRepositorio.ObterPorId(id, IdUsuario);
                if (quote is null)
                {
                    _notificacoes.AddNotificacao("ORCAMENTO", $"Orçamento {id} não encontrado.");
                    return CreateResponse(HttpStatusCode.NotFound, new RetornoBase(HttpStatusCode.NotFound, _notificacoes.Notificacoes));
                }

                if (string.IsNullOrWhiteSpace(quote.CustomerPhone))
                {
                    _notificacoes.AddNotificacao("CLIENTE", "O cliente deste orçamento não tem telefone cadastrado.");
                    return CreateResponse(HttpStatusCode.BadRequest, new RetornoBase(HttpStatusCode.BadRequest, _notificacoes.Notificacoes));
                }

                var phoneDigits = new string(quote.CustomerPhone.Where(char.IsDigit).ToArray());
                if (!phoneDigits.StartsWith("55")) phoneDigits = "55" + phoneDigits;

                var message = MontarMensagem(quote);
                var url = $"https://wa.me/{phoneDigits}?text={Uri.EscapeDataString(message)}";

                return CreateResponse(HttpStatusCode.OK, new RetornoBase(HttpStatusCode.OK, _notificacoes.Notificacoes, new WhatsAppLinkDto(url, message)));
            }
            catch (Exception ex)
            {
                return LogErrorReturnInternalServerError(ex, _log, _notificacoes, "gerar link do WhatsApp", lambdaContext);
            }
        }

        private static string MontarMensagem(Quote quote)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Olá {quote.CustomerName}, segue seu orçamento:");
            sb.AppendLine();
            foreach (var item in quote.Items)
                sb.AppendLine($"- {item.Quantity}x {item.Description}: R$ {item.Total:F2}");
            sb.AppendLine();
            sb.AppendLine($"Total: R$ {quote.Total:F2}");
            if (quote.ValidUntil is not null)
                sb.AppendLine($"Válido até: {quote.ValidUntil:dd/MM/yyyy}");
            return sb.ToString();
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
