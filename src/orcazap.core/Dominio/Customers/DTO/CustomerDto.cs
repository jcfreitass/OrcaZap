namespace core.Dominio.Customers.DTO
{
    public class CustomerDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }

        public static CustomerDto DePara(Customer c) => new()
        {
            Id = c.Id,
            UserId = c.UserId,
            Name = c.Name,
            Phone = c.Phone,
            Email = c.Email,
            CreatedAt = c.CreatedAt
        };

        public static List<CustomerDto> DePara(IEnumerable<Customer> customers) =>
            customers.Select(DePara).ToList();
    }
}
