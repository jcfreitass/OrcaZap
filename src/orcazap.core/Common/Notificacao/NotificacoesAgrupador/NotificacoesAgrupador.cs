namespace core.Common.Notificacao.NotificacoesAgrupador
{
    public class NotificacoesAgrupador
    {
        public List<Notificacao> Notificacoes { get; } = new();

        public void AddNotificacao(string chave, string mensagem)
        {
            Notificacoes.Add(new Notificacao(chave, mensagem));
        }

        public bool TemNotificacao => Notificacoes.Count > 0;
    }
}
