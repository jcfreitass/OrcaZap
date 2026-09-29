namespace core.Dominio.Quotes.DTO
{
    public class WhatsAppLinkDto
    {
        public string Url { get; set; }
        public string Message { get; set; }

        public WhatsAppLinkDto() { }

        public WhatsAppLinkDto(string url, string message)
        {
            Url = url;
            Message = message;
        }
    }
}
