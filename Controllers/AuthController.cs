using Microsoft.AspNetCore.Mvc;
using ClinicaMedica.Authentication;
using ClinicaMedica.Models;
using ClinicaMedica.Services;

namespace ClinicaMedica.Controllers
{
    [ApiController]
    [Route("api/clinica/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly IUserContextService _userContext;

        public AuthController(AuthService authService, IUserContextService userContext)
        {
            _authService = authService;
            _userContext = userContext;
        }

        /// Iniciar sesión para obtener el Token JWT con sus correspondientes Claims.
        [HttpPost("login")]
        public async Task<GeneralResponse<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            return await _authService.LoginAsync(request);
        }
    }
}

