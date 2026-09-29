using core.Dominio.Users;
using core.Infrastructure.Data.MySql;
using Dapper;

namespace core.Infrastructure.Repositorios.MySql
{
    public class UserRepositorio : RepositorioBase, IUserRepositorio
    {
        public UserRepositorio(Func<int, MySqlDbConnection> factoryConexao)
            : base(factoryConexao) { }

        public IEnumerable<User> ObterTodos()
        {
            using var conn = _conexao.ObterConexaoLeitura();
            return conn.Query<User>(
                "SELECT id, name, email, password_hash, created_at FROM users ORDER BY id");
        }

        public User ObterPorId(long id)
        {
            using var conn = _conexao.ObterConexaoLeitura();
            return conn.QueryFirstOrDefault<User>(
                "SELECT id, name, email, password_hash, created_at FROM users WHERE id = @id",
                new { id });
        }

        public User ObterPorEmail(string email)
        {
            using var conn = _conexao.ObterConexaoLeitura();
            return conn.QueryFirstOrDefault<User>(
                "SELECT id, name, email, password_hash, created_at FROM users WHERE email = @email",
                new { email });
        }

        public long Inserir(User user)
        {
            using var conn = _conexao.ObterConexaoEscrita();
            return conn.ExecuteScalar<long>(
                @"INSERT INTO users (name, email, password_hash, created_at)
                  VALUES (@Name, @Email, @PasswordHash, @CreatedAt);
                  SELECT LAST_INSERT_ID();",
                user);
        }
    }
}
