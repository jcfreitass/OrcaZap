namespace core.Dominio.Quotes.DTO
{
    public class QuoteItemDto
    {
        public long Id { get; set; }
        public long? ServiceId { get; set; }
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }

        public static QuoteItemDto DePara(QuoteItem i) => new()
        {
            Id = i.Id,
            ServiceId = i.ServiceId,
            Description = i.Description,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            Total = i.Total
        };
    }
}
