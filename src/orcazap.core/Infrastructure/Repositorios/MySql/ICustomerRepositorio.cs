using core.Dominio.Customers;

namespace core.Infrastructure.Repositorios.MySql
{
    public interface ICustomerRepositorio
    {
        IEnumerable<Customer> ObterTodos(long? userId = null);
        Customer ObterPorId(long id, long userId);
        long Inserir(Customer customer);
        bool Atualizar(Customer customer);
        bool Remover(long id, long userId);
    }
}
