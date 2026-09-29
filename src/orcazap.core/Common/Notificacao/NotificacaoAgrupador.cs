namespace core.Common.Notificacao
{
    public class NotificacaoAgrupador
    {
        public List<Notificacao> Notificacoes { get; } = new();

        public void AddNotificacao(string mensagem)
        {
            Notificacoes.Add(new Notificacao(mensagem));
        }

        public bool TemNotificacao => Notificacoes.Count > 0;
    }
}
