using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ClinicaMedica.Models;

namespace ClinicaMedica.Authentication
{
    public class JwtTokenService : ITokenService
    {
        private readonly string _jwtKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expireMinutes;

        public JwtTokenService(IConfiguration configuration)
        {
            _jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") 
                      ?? configuration["JWT_KEY"] 
                      ?? configuration["Jwt:Key"]
                      ?? "ClaveSecretaSuperSeguraParaClinicaMedica2026!@#$%";

            _issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") 
                      ?? configuration["JWT_ISSUER"] 
                      ?? "ClinicaMedicaApi";

            _audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") 
                        ?? configuration["JWT_AUDIENCE"] 
                        ?? "ClinicaMedicaClients";

            var expireStr = Environment.GetEnvironmentVariable("JWT_EXPIRE_MINUTES") 
                            ?? configuration["Jwt:ExpireMinutes"] 
                            ?? "120";

            _expireMinutes = int.TryParse(expireStr, out var mins) ? mins : 120;
        }

        public string GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var keyBytes = Encoding.UTF8.GetBytes(_jwtKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("userId", user.UserId.ToString()),
                new Claim("username", user.Username),
                new Claim("fullName", user.FullName),
                new Claim("role", user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_expireMinutes),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(keyBytes),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var keyBytes = Encoding.UTF8.GetBytes(_jwtKey);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken _);

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}

