using core.Dominio.Users;

namespace core.Infrastructure.Repositorios.MySql
{
    public interface IUserRepositorio
    {
        IEnumerable<User> ObterTodos();
        User ObterPorId(long id);
        User ObterPorEmail(string email);
        long Inserir(User user);
    }
}
