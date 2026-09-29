using core.Attributes;

namespace core.Dominio.Quotes.DTO
{
    public class CreateQuoteItemDto
    {
        public long? ServiceId { get; set; }

        [CampoObrigatorio("Descrição do item é obrigatória.")]
        public string Description { get; set; }

        [CampoObrigatorio("Quantidade é obrigatória.")]
        public decimal Quantity { get; set; }

        [CampoObrigatorio("Preço unitário é obrigatório.")]
        public decimal UnitPrice { get; set; }
    }
}
