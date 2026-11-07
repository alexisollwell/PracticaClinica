using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ClinicaMedica.Models;

namespace ClinicaMedica.Authentication
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class ApiKeyAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private const string ApiKeyHeaderName = "X-API-KEY";

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var apiKeyValidator = context.HttpContext.RequestServices.GetRequiredService<IApiKeyValidator>();

            if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
            {
                context.Result = new ObjectResult(new GeneralResponse<object>
                {
                    Success = false,
                    Message = $"Cabecera '{ApiKeyHeaderName}' requerida para este endpoint."
                })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            var isValid = await apiKeyValidator.ValidateApiKeyAsync(extractedApiKey.ToString());
            if (!isValid)
            {
                context.Result = new ObjectResult(new GeneralResponse<object>
                {
                    Success = false,
                    Message = "API Key inválida o inactiva."
                })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
        }
    }
}

