using core.Attributes;

namespace core.Dominio.Quotes.DTO
{
    public class CreateQuoteDto
    {
        [CampoObrigatorio("CustomerId é obrigatório.")]
        public long CustomerId { get; set; }

        public DateTime? ValidUntil { get; set; }

        [CampoObrigatorio("Ao menos um item é obrigatório.")]
        public List<CreateQuoteItemDto> Items { get; set; } = new();
    }
}
