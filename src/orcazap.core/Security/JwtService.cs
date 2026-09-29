using core.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace core.Security
{
    public class JwtService : IJwtService
    {
        private const int EXPIRACAO_HORAS = 8;
        private readonly JwtSettings _settings;

        public JwtService(JwtSettings settings)
        {
            _settings = settings;
        }

        public string GerarToken(long idUsuario, string email)
        {
            var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret()));
            var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, idUsuario.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email ?? string.Empty)
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(EXPIRACAO_HORAS),
                signingCredentials: credenciais);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public TokenUsuario ValidarToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;

            var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret()));
            var parametros = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = chave,
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var handler = new JwtSecurityTokenHandler();
                handler.InboundClaimTypeMap.Clear();
                var principal = handler.ValidateToken(token, parametros, out _);
                var idUsuario = long.Parse(principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? throw new InvalidOperationException());
                var email = principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
                return new TokenUsuario(idUsuario, email);
            }
            catch
            {
                return null;
            }
        }
    }
}
