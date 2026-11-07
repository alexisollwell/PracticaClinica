using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ClinicaMedica.Models;

namespace ClinicaMedica.Authentication
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class AuthorizeTokenAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _requiredRoles;

        public AuthorizeTokenAttribute(params string[] requiredRoles)
        {
            _requiredRoles = requiredRoles ?? Array.Empty<string>();
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var tokenService = context.HttpContext.RequestServices.GetRequiredService<ITokenService>();
            var authHeader = context.HttpContext.Request.Headers.Authorization.ToString();

            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new ObjectResult(new GeneralResponse<object>
                {
                    Success = false,
                    Message = "Token Bearer no proporcionado en la cabecera Authorization."
                })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            var token = authHeader["Bearer ".Length..].Trim();
            var principal = tokenService.ValidateToken(token);

            if (principal == null)
            {
                context.Result = new ObjectResult(new GeneralResponse<object>
                {
                    Success = false,
                    Message = "Token inválido o expirado."
                })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            // Inyectamos el usuario validado en el HttpContext para que los servicios lean los Claims
            context.HttpContext.User = principal;

            // Validación de Roles si se especificaron
            if (_requiredRoles.Length > 0)
            {
                var userRole = principal.FindFirst(ClaimTypes.Role)?.Value 
                               ?? principal.FindFirst("role")?.Value;

                var isAuthorized = _requiredRoles.Any(r =>
                    string.Equals(r.Trim(), userRole, StringComparison.OrdinalIgnoreCase));

                if (!isAuthorized)
                {
                    context.Result = new ObjectResult(new GeneralResponse<object>
                    {
                        Success = false,
                        Message = $"Acceso denegado. Se requiere uno de los siguientes roles: {string.Join(", ", _requiredRoles)}."
                    })
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                }
            }
        }
    }
}

