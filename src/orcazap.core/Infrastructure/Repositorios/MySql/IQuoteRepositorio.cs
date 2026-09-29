using core.Dominio.Enums;
using core.Dominio.Quotes;

namespace core.Infrastructure.Repositorios.MySql
{
    public interface IQuoteRepositorio
    {
        IEnumerable<Quote> ObterTodos(long? userId = null);
        Quote ObterPorId(long id, long userId);
        long Inserir(Quote quote);
        bool AtualizarStatus(long id, EQuoteStatus status, long userId);
        bool Remover(long id, long userId);
    }
}
