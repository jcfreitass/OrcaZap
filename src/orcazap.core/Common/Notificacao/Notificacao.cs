namespace core.Common.Notificacao
{
    public class Notificacao
    {
        public string Chave { get; set; }
        public string Mensagem { get; set; }

        public Notificacao() { }

        public Notificacao(string chave, string mensagem)
        {
            Chave = chave;
            Mensagem = mensagem;
        }

        public Notificacao(string mensagem)
        {
            Mensagem = mensagem;
        }
    }
}
