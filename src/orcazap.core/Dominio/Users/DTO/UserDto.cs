namespace core.Dominio.Users.DTO
{
    public class UserDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }

        public static UserDto DePara(User user) => new()
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            CreatedAt = user.CreatedAt
        };

        public static List<UserDto> DePara(IEnumerable<User> users) =>
            users.Select(DePara).ToList();
    }
}
