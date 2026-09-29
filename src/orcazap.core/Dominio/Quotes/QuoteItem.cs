namespace core.Dominio.Quotes
{
    public class QuoteItem
    {
        public long Id { get; set; }
        public long QuoteId { get; set; }
        public long? ServiceId { get; set; }
        public string Description { get; set; }
        public decimal Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
    }
}
