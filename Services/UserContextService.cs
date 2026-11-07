using System.Security.Claims;

namespace ClinicaMedica.Services
{
    /// <summary>
    /// Servicio que extrae de forma segura los Claims del usuario autenticado
    /// desde el HttpContext del request actual (inyectado por AuthorizeTokenAttribute).
    /// </summary>
    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ClaimsPrincipal? GetUser()
        {
            return _httpContextAccessor.HttpContext?.User;
        }

        public bool IsAuthenticated()
        {
            var user = GetUser();
            return user?.Identity?.IsAuthenticated == true;
        }

        public int? GetUserId()
        {
            var user = GetUser();
            var val = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                      ?? user?.FindFirst("userId")?.Value;

            return int.TryParse(val, out var id) ? id : null;
        }

        public string? GetUsername()
        {
            var user = GetUser();
            return user?.FindFirst(ClaimTypes.Name)?.Value 
                   ?? user?.FindFirst("username")?.Value;
        }

        public string? GetFullName()
        {
            var user = GetUser();
            return user?.FindFirst("fullName")?.Value 
                   ?? user?.FindFirst(ClaimTypes.GivenName)?.Value;
        }

        public string? GetEmail()
        {
            var user = GetUser();
            return user?.FindFirst(ClaimTypes.Email)?.Value 
                   ?? user?.FindFirst("email")?.Value;
        }

        public string? GetRole()
        {
            var user = GetUser();
            return user?.FindFirst(ClaimTypes.Role)?.Value 
                   ?? user?.FindFirst("role")?.Value;
        }

        public bool IsInRole(string role)
        {
            var currentRole = GetRole();
            return string.Equals(currentRole, role, StringComparison.OrdinalIgnoreCase);
        }

        public string? GetClaim(string claimType)
        {
            return GetUser()?.FindFirst(claimType)?.Value;
        }
    }
}

