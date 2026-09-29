using core.Dominio.Enums;
using core.Dominio.Quotes;
using core.Infrastructure.Data.MySql;
using Dapper;

namespace core.Infrastructure.Repositorios.MySql
{
    public class QuoteRepositorio : RepositorioBase, IQuoteRepositorio
    {
        public QuoteRepositorio(Func<int, MySqlDbConnection> factoryConexao)
            : base(factoryConexao) { }

        public IEnumerable<Quote> ObterTodos(long? userId = null)
        {
            using var conn = _conexao.ObterConexaoLeitura();
            var quoteSql = @"SELECT q.id, q.user_id, q.customer_id, q.status, q.total, q.valid_until, q.created_at,
                                    c.name AS customer_name, c.phone AS customer_phone
                             FROM quotes q
                             INNER JOIN customers c ON c.id = q.customer_id
                             WHERE (@userId IS NULL OR q.user_id = @userId)
                             ORDER BY q.id";
            var quotes = conn.Query<Quote>(quoteSql, new { userId }).ToList();
            if (quotes.Count == 0) return quotes;

            var ids = quotes.Select(q => q.Id).ToArray();
            var items = conn.Query<QuoteItem>(
                @"SELECT id, quote_id, service_id, description, quantity, unit_price, total
                  FROM quote_items WHERE quote_id IN @ids",
                new { ids }).ToList();

            foreach (var q in quotes)
                q.Items = items.Where(i => i.QuoteId == q.Id).ToList();

            return quotes;
        }

        public Quote ObterPorId(long id, long userId)
        {
            using var conn = _conexao.ObterConexaoLeitura();
            var quote = conn.QueryFirstOrDefault<Quote>(
                @"SELECT q.id, q.user_id, q.customer_id, q.status, q.total, q.valid_until, q.created_at,
                         c.name AS customer_name, c.phone AS customer_phone
                  FROM quotes q
                  INNER JOIN customers c ON c.id = q.customer_id
                  WHERE q.id = @id AND q.user_id = @userId",
                new { id, userId });

            if (quote is null) return null;

            quote.Items = conn.Query<QuoteItem>(
                @"SELECT id, quote_id, service_id, description, quantity, unit_price, total
                  FROM quote_items WHERE quote_id = @id",
                new { id }).ToList();

            return quote;
        }

        public long Inserir(Quote quote)
        {
            using var conn = _conexao.ObterConexaoEscrita();
            conn.Open();
            using var tx = conn.BeginTransaction();

            var quoteId = conn.ExecuteScalar<long>(
                @"INSERT INTO quotes (user_id, customer_id, status, total, valid_until, created_at)
                  VALUES (@UserId, @CustomerId, @Status, @Total, @ValidUntil, @CreatedAt);
                  SELECT LAST_INSERT_ID();",
                new
                {
                    quote.UserId,
                    quote.CustomerId,
                    Status = quote.Status.ToString().ToLowerInvariant(),
                    quote.Total,
                    ValidUntil = quote.ValidUntil?.Date,
                    quote.CreatedAt
                },
                tx);

            foreach (var item in quote.Items)
            {
                item.QuoteId = quoteId;
                conn.Execute(
                    @"INSERT INTO quote_items (quote_id, service_id, description, quantity, unit_price, total)
                      VALUES (@QuoteId, @ServiceId, @Description, @Quantity, @UnitPrice, @Total);",
                    item, tx);
            }

            tx.Commit();
            return quoteId;
        }

        public bool AtualizarStatus(long id, EQuoteStatus status, long userId)
        {
            using var conn = _conexao.ObterConexaoEscrita();
            var rows = conn.Execute(
                "UPDATE quotes SET status = @status WHERE id = @id AND user_id = @userId",
                new { id, status = status.ToString().ToLowerInvariant(), userId });
            return rows > 0;
        }

        public bool Remover(long id, long userId)
        {
            using var conn = _conexao.ObterConexaoEscrita();
            conn.Open();
            using var tx = conn.BeginTransaction();
            var rows = conn.Execute(
                "DELETE qi FROM quote_items qi INNER JOIN quotes q ON q.id = qi.quote_id WHERE q.id = @id AND q.user_id = @userId",
                new { id, userId }, tx);
            rows = conn.Execute("DELETE FROM quotes WHERE id = @id AND user_id = @userId", new { id, userId }, tx);
            tx.Commit();
            return rows > 0;
        }
    }
}
