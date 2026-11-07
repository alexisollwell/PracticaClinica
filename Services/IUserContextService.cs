using System.Security.Claims;

namespace ClinicaMedica.Services
{
    public interface IUserContextService
    {
        ClaimsPrincipal? GetUser();
        bool IsAuthenticated();
        int? GetUserId();
        string? GetUsername();
        string? GetFullName();
        string? GetEmail();
        string? GetRole();
        bool IsInRole(string role);
        string? GetClaim(string claimType);
    }
}

