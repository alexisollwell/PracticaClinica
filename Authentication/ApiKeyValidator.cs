using System.Data;
using Microsoft.Data.SqlClient;

namespace ClinicaMedica.Authentication
{
    public class ApiKeyValidator : IApiKeyValidator
    {
        private readonly string _connectionString;

        public ApiKeyValidator(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ClinicaConnection")
                                ?? configuration.GetConnectionString("DefaultConnection")
                                ?? throw new InvalidOperationException("Cadena de conexión no configurada.");
        }

        public async Task<bool> ValidateApiKeyAsync(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || !Guid.TryParse(apiKey, out var apiKeyGuid))
            {
                return false;
            }

            try
            {
                using var connection = new SqlConnection(_connectionString);
                const string query = "SELECT COUNT(1) FROM ApiKeys WHERE KeyValue = @KeyValue AND IsActive = 1";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@KeyValue", apiKeyGuid);

                await connection.OpenAsync();
                var count = Convert.ToInt32(await command.ExecuteScalarAsync() ?? 0);
                return count > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}

