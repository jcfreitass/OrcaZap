using core.Dominio.Services;

namespace core.Infrastructure.Repositorios.MySql
{
    public interface IServiceRepositorio
    {
        IEnumerable<Service> ObterTodos(long? userId = null);
        Service ObterPorId(long id, long userId);
        long Inserir(Service service);
        bool Atualizar(Service service);
        bool Remover(long id, long userId);
    }
}
