using ClinicaMedica.Authentication;
using ClinicaMedica.Database;
using ClinicaMedica.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ClinicaMedicaServiceExtensions
    {
        /// <summary>
        /// Registra en el contenedor de Inyección de Dependencias
        /// todos los componentes requeridos para el módulo de Clínica Médica.
        /// </summary>
        public static IServiceCollection AddClinicaMedicaModule(this IServiceCollection services)
        {
            // 1. Acceso al HttpContext para lectura de Claims
            services.AddHttpContextAccessor();

            // 2. Repositorios (ADO.NET)
            services.AddScoped<DoctorRepository>();
            services.AddScoped<AppointmentRepository>();
            services.AddScoped<UserRepository>();

            // 3. Seguridad y Tokens
            services.AddScoped<ITokenService, JwtTokenService>();
            services.AddScoped<IApiKeyValidator, ApiKeyValidator>();

            // 4. Servicios de Negocio y Lector de Claims
            services.AddScoped<IUserContextService, UserContextService>();
            services.AddScoped<AppointmentService>();
            services.AddScoped<AuthService>();

            return services;
        }
    }
}

