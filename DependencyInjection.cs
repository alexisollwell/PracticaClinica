using ClinicaMedica.Authentication;
using ClinicaMedica.Database;
using ClinicaMedica.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ClinicaMedicaServiceExtensions
    {
        public static IServiceCollection AddClinicaMedicaModule(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<DoctorRepository>();
            services.AddScoped<AppointmentRepository>();
            services.AddScoped<UserRepository>();
            services.AddScoped<ITokenService, JwtTokenService>();
            services.AddScoped<IApiKeyValidator, ApiKeyValidator>();
            services.AddScoped<IUserContextService, UserContextService>();
            services.AddScoped<AppointmentService>();
            services.AddScoped<AuthService>();

            return services;
        }
    }
}

