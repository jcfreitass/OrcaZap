using core.Attributes;

namespace core.Dominio.Customers.DTO
{
    public class UpdateCustomerDto
    {
        [CampoObrigatorio("Nome é obrigatório.")]
        public string Name { get; set; }

        public string Phone { get; set; }
        public string Email { get; set; }
    }
}
