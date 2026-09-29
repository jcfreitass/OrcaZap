using core.Dominio.Enums;

namespace core.Dominio.Quotes.DTO
{
    public class QuoteDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long CustomerId { get; set; }
        public string CustomerName { get; set; }
        public EQuoteStatus Status { get; set; }
        public decimal Total { get; set; }
        public DateTime? ValidUntil { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<QuoteItemDto> Items { get; set; } = new();

        public static QuoteDto DePara(Quote q) => new()
        {
            Id = q.Id,
            UserId = q.UserId,
            CustomerId = q.CustomerId,
            CustomerName = q.CustomerName,
            Status = q.Status,
            Total = q.Total,
            ValidUntil = q.ValidUntil,
            CreatedAt = q.CreatedAt,
            Items = q.Items?.Select(QuoteItemDto.DePara).ToList() ?? new List<QuoteItemDto>()
        };

        public static List<QuoteDto> DePara(IEnumerable<Quote> quotes) =>
            quotes.Select(DePara).ToList();
    }
}
