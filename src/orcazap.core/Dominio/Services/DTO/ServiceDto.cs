namespace core.Dominio.Services.DTO
{
    public class ServiceDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }

        public static ServiceDto DePara(Service s) => new()
        {
            Id = s.Id,
            UserId = s.UserId,
            Name = s.Name,
            Description = s.Description,
            Price = s.Price,
            CreatedAt = s.CreatedAt
        };

        public static List<ServiceDto> DePara(IEnumerable<Service> services) =>
            services.Select(DePara).ToList();
    }
}
