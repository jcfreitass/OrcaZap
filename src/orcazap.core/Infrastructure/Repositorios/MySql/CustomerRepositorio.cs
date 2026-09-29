using core.Dominio.Customers;
using core.Infrastructure.Data.MySql;
using Dapper;

namespace core.Infrastructure.Repositorios.MySql
{
    public class CustomerRepositorio : RepositorioBase, ICustomerRepositorio
    {
        public CustomerRepositorio(Func<int, MySqlDbConnection> factoryConexao)
            : base(factoryConexao) { }

        public IEnumerable<Customer> ObterTodos(long? userId = null)
        {
            using var conn = _conexao.ObterConexaoLeitura();
            var sql = @"SELECT id, user_id, name, phone, email, created_at
                        FROM customers
                        WHERE (@userId IS NULL OR user_id = @userId)
                        ORDER BY id";
            return conn.Query<Customer>(sql, new { userId });
        }

        public Customer ObterPorId(long id, long userId)
        {
            using var conn = _conexao.ObterConexaoLeitura();
            return conn.QueryFirstOrDefault<Customer>(
                "SELECT id, user_id, name, phone, email, created_at FROM customers WHERE id = @id AND user_id = @userId",
                new { id, userId });
        }

        public long Inserir(Customer customer)
        {
            using var conn = _conexao.ObterConexaoEscrita();
            return conn.ExecuteScalar<long>(
                @"INSERT INTO customers (user_id, name, phone, email, created_at)
                  VALUES (@UserId, @Name, @Phone, @Email, @CreatedAt);
                  SELECT LAST_INSERT_ID();",
                customer);
        }

        public bool Atualizar(Customer customer)
        {
            using var conn = _conexao.ObterConexaoEscrita();
            var rows = conn.Execute(
                @"UPDATE customers
                  SET name = @Name, phone = @Phone, email = @Email
                  WHERE id = @Id AND user_id = @UserId",
                customer);
            return rows > 0;
        }

        public bool Remover(long id, long userId)
        {
            using var conn = _conexao.ObterConexaoEscrita();
            var rows = conn.Execute("DELETE FROM customers WHERE id = @id AND user_id = @userId", new { id, userId });
            return rows > 0;
        }
    }
}
