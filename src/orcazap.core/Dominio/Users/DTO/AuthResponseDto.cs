namespace core.Dominio.Users.DTO
{
    public class AuthResponseDto
    {
        public string Token { get; set; }
        public UserDto User { get; set; }

        public static AuthResponseDto Criar(string token, User user) => new()
        {
            Token = token,
            User = UserDto.DePara(user)
        };
    }
}
