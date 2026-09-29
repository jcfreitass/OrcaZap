namespace core.Security
{
    public record TokenUsuario(long IdUsuario, string Email);

    public interface IJwtService
    {
        string GerarToken(long idUsuario, string email);
        TokenUsuario ValidarToken(string token);
    }
}
