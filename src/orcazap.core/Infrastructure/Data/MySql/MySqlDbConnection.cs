using core.Configuration;
using MySqlConnector;

namespace core.Infrastructure.Data.MySql
{
    public class MySqlDbConnection
    {
        private readonly int _servidor;

        public MySqlDbConnection() { }

        public MySqlDbConnection(int servidor)
        {
            _servidor = servidor;
        }

        public MySqlConnection ObterConexaoLeitura()
        {
            return new MySqlConnection(new DbSettings("connection_orcazap_mysql_read").ConnectionString());
        }

        public MySqlConnection ObterConexaoEscrita()
        {
            return new MySqlConnection(new DbSettings("connection_orcazap_mysql_write").ConnectionString());
        }
    }
}
