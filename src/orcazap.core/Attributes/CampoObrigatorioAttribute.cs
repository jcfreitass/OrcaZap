namespace core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CampoObrigatorioAttribute : Attribute
    {
        public string Mensagem { get; }

        public CampoObrigatorioAttribute(string mensagem = null)
        {
            Mensagem = mensagem;
        }
    }
}
