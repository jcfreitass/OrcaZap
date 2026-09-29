using core.Dominio.Enums;

namespace core.Dominio.Quotes
{
    public class Quote
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long CustomerId { get; set; }
        public EQuoteStatus Status { get; set; } = EQuoteStatus.Draft;
        public decimal Total { get; set; }
        public DateTime? ValidUntil { get; set; }
        public DateTime CreatedAt { get; set; }

        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public List<QuoteItem> Items { get; set; } = new();
    }
}
