using core.Attributes;

namespace core.Dominio.Customers.DTO
{
    public class CreateCustomerDto
    {
        [CampoObrigatorio("Nome é obrigatório.")]
        public string Name { get; set; }

        public string Phone { get; set; }
        public string Email { get; set; }
    }
}
