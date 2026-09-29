using core.Attributes;

namespace core.Dominio.Users.DTO
{
    public class LoginDto
    {
        [CampoObrigatorio("E-mail é obrigatório.")]
        public string Email { get; set; }

        [CampoObrigatorio("Senha é obrigatória.")]
        public string Password { get; set; }
    }
}
