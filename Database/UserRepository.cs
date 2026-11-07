using System.Data;
using Microsoft.Data.SqlClient;
using ClinicaMedica.Models;

namespace ClinicaMedica.Database
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ClinicaConnection")
                                ?? configuration.GetConnectionString("DefaultConnection")
                                ?? throw new InvalidOperationException("Cadena de conexión no configurada.");
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            using var connection = new SqlConnection(_connectionString);
            const string query = @"SELECT UserId, Username, FullName, Email, PasswordHash, Role 
                                   FROM Users 
                                   WHERE Username = @Username";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Username", username);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    UserId = Convert.ToInt32(reader["UserId"]),
                    Username = reader["Username"].ToString() ?? "",
                    FullName = reader["FullName"].ToString() ?? "",
                    Email = reader["Email"].ToString() ?? "",
                    PasswordHash = reader["PasswordHash"].ToString() ?? "",
                    Role = reader["Role"].ToString() ?? "Patient"
                };
            }

            return null;
        }
    }
}

