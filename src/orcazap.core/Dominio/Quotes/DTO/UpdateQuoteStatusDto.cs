using core.Attributes;
using core.Dominio.Enums;

namespace core.Dominio.Quotes.DTO
{
    public class UpdateQuoteStatusDto
    {
        [CampoObrigatorio("Status é obrigatório.")]
        public EQuoteStatus Status { get; set; }
    }
}
