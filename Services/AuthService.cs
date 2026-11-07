using ClinicaMedica.Authentication;
using ClinicaMedica.Database;
using ClinicaMedica.Models;

namespace ClinicaMedica.Services
{
    public class AuthService
    {
        private readonly UserRepository _userRepository;
        private readonly ITokenService _tokenService;

        public AuthService(UserRepository userRepository, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        public async Task<GeneralResponse<LoginResponse>> LoginAsync(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return new GeneralResponse<LoginResponse>
                {
                    Success = false,
                    Message = "Usuario y contraseña requeridos."
                };
            }

            var user = await _userRepository.GetByUsernameAsync(request.Username);

            // En un entorno de producción se compara con hash PBKDF2/BCrypt
            if (user == null || user.PasswordHash != request.Password)
            {
                return new GeneralResponse<LoginResponse>
                {
                    Success = false,
                    Message = "Credenciales incorrectas."
                };
            }

            var token = _tokenService.GenerateToken(user);

            return new GeneralResponse<LoginResponse>
            {
                Success = true,
                Message = "Inicio de sesión exitoso.",
                Data = new LoginResponse
                {
                    Token = token,
                    Username = user.Username,
                    FullName = user.FullName,
                    Role = user.Role
                }
            };
        }
    }
}

