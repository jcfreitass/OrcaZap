using core.Attributes;

namespace core.Dominio.Services.DTO
{
    public class UpdateServiceDto
    {
        [CampoObrigatorio("Nome do serviço é obrigatório.")]
        public string Name { get; set; }

        public string Description { get; set; }

        [CampoObrigatorio("Preço é obrigatório.")]
        public decimal Price { get; set; }
    }
}
