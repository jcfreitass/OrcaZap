using core.Infrastructure.Data.MySql;

namespace core.Infrastructure
{
    public abstract class RepositorioBase
    {
        protected readonly MySqlDbConnection _conexao;

        protected RepositorioBase(Func<int, MySqlDbConnection> factoryConexao)
        {
            _conexao = factoryConexao(0);
        }
    }
}
