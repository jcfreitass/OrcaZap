using core.Dominio.Services;
using core.Infrastructure.Data.MySql;
using Dapper;

namespace core.Infrastructure.Repositorios.MySql
{
    public class ServiceRepositorio : RepositorioBase, IServiceRepositorio
    {
        public ServiceRepositorio(Func<int, MySqlDbConnection> factoryConexao)
            : base(factoryConexao) { }

        public IEnumerable<Service> ObterTodos(long? userId = null)
        {
            using var conn = _conexao.ObterConexaoLeitura();
            var sql = @"SELECT id, user_id, name, description, price, created_at
                        FROM services
                        WHERE (@userId IS NULL OR user_id = @userId)
                        ORDER BY id";
            return conn.Query<Service>(sql, new { userId });
        }

        public Service ObterPorId(long id, long userId)
        {
            using var conn = _conexao.ObterConexaoLeitura();
            return conn.QueryFirstOrDefault<Service>(
                "SELECT id, user_id, name, description, price, created_at FROM services WHERE id = @id AND user_id = @userId",
                new { id, userId });
        }

        public long Inserir(Service service)
        {
            using var conn = _conexao.ObterConexaoEscrita();
            return conn.ExecuteScalar<long>(
                @"INSERT INTO services (user_id, name, description, price, created_at)
                  VALUES (@UserId, @Name, @Description, @Price, @CreatedAt);
                  SELECT LAST_INSERT_ID();",
                service);
        }

        public bool Atualizar(Service service)
        {
            using var conn = _conexao.ObterConexaoEscrita();
            var rows = conn.Execute(
                @"UPDATE services
                  SET name = @Name, description = @Description, price = @Price
                  WHERE id = @Id AND user_id = @UserId",
                service);
            return rows > 0;
        }

        public bool Remover(long id, long userId)
        {
            using var conn = _conexao.ObterConexaoEscrita();
            var rows = conn.Execute("DELETE FROM services WHERE id = @id AND user_id = @userId", new { id, userId });
            return rows > 0;
        }
    }
}
