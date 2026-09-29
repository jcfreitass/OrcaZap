namespace core.Infrastructure
{
    public interface IRepositorio<T> where T : class
    {
        IEnumerable<T> ObterTodos();
        T ObterPorId(long id);
        long Inserir(T entidade);
        bool Atualizar(T entidade);
        bool Remover(long id);
    }
}
