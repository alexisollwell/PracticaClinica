using System.Security.Claims;
using ClinicaMedica.Models;

namespace ClinicaMedica.Authentication
{
    public interface ITokenService
    {
        string GenerateToken(User user);
        ClaimsPrincipal? ValidateToken(string token);
    }
}

