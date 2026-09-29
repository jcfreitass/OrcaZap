using System.Net;
using NotificacaoModel = core.Common.Notificacao.Notificacao;

namespace core.Dominio.Retorno
{
    public class RetornoBase
    {
        public HttpStatusCode StatusCode { get; set; }
        public List<NotificacaoModel> Notificacoes { get; set; }
        public object Data { get; set; }

        public RetornoBase() { }

        public RetornoBase(HttpStatusCode statusCode, List<NotificacaoModel> notificacoes = null, object data = null)
        {
            StatusCode = statusCode;
            Notificacoes = notificacoes ?? new List<NotificacaoModel>();
            Data = data;
        }
    }
}
