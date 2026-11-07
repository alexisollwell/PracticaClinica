using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ClinicaMedica.Models;

namespace ClinicaMedica.Authentication
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class AuthorizeApiKeyOrTokenAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private const string ApiKeyHeaderName = "X-API-KEY";

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var tokenService = context.HttpContext.RequestServices.GetRequiredService<ITokenService>();
            var apiKeyValidator = context.HttpContext.RequestServices.GetRequiredService<IApiKeyValidator>();

            // 1. Intentar validar por Bearer Token
            var authHeader = context.HttpContext.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader["Bearer ".Length..].Trim();
                var principal = tokenService.ValidateToken(token);
                if (principal != null)
                {
                    context.HttpContext.User = principal;
                    return; // Autorizado por JWT
                }
            }

            // 2. Intentar validar por API Key
            if (context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
            {
                var isValid = await apiKeyValidator.ValidateApiKeyAsync(extractedApiKey.ToString());
                if (isValid)
                {
                    return; // Autorizado por API Key
                }
            }

            // Si ambas fallan
            context.Result = new ObjectResult(new GeneralResponse<object>
            {
                Success = false,
                Message = "Acceso no autorizado. Debe proporcionar un Bearer Token válido o un header X-API-KEY válido."
            })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
        }
    }
}

