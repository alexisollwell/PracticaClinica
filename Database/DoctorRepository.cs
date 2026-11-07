using System.Data;
using Microsoft.Data.SqlClient;
using ClinicaMedica.Models;

namespace ClinicaMedica.Database
{
    public class DoctorRepository
    {
        private readonly string _connectionString;

        public DoctorRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ClinicaConnection")
                                ?? configuration.GetConnectionString("DefaultConnection")
                                ?? throw new InvalidOperationException("Cadena de conexión no encontrada.");
        }

        public async Task<List<Specialty>> GetActiveSpecialtiesAsync()
        {
            var list = new List<Specialty>();
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_GetActiveSpecialties", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new Specialty
                {
                    SpecialtyId = Convert.ToInt32(reader["SpecialtyId"]),
                    Name = reader["Name"].ToString() ?? "",
                    Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : null,
                    ConsultationFee = Convert.ToDecimal(reader["ConsultationFee"])
                });
            }

            return list;
        }

        public async Task<List<Doctor>> GetActiveDoctorsAsync(int? specialtyId = null)
        {
            var list = new List<Doctor>();
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_GetActiveDoctors", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            if (specialtyId.HasValue)
                command.Parameters.AddWithValue("@SpecialtyId", specialtyId.Value);
            else
                command.Parameters.AddWithValue("@SpecialtyId", DBNull.Value);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new Doctor
                {
                    DoctorId = Convert.ToInt32(reader["DoctorId"]),
                    UserId = Convert.ToInt32(reader["UserId"]),
                    DoctorName = reader["DoctorName"].ToString() ?? "",
                    SpecialtyId = Convert.ToInt32(reader["SpecialtyId"]),
                    SpecialtyName = reader["SpecialtyName"].ToString() ?? "",
                    ProfessionalLicense = reader["ProfessionalLicense"].ToString() ?? "",
                    ConsultationRoom = reader["ConsultationRoom"].ToString() ?? "",
                    WorkStartTime = (TimeSpan)reader["WorkStartTime"],
                    WorkEndTime = (TimeSpan)reader["WorkEndTime"],
                    SlotDurationMinutes = Convert.ToInt32(reader["SlotDurationMinutes"]),
                    ConsultationFee = Convert.ToDecimal(reader["ConsultationFee"])
                });
            }

            return list;
        }

        public async Task<Doctor?> GetDoctorByIdAsync(int doctorId)
        {
            var doctors = await GetActiveDoctorsAsync(null);
            return doctors.FirstOrDefault(d => d.DoctorId == doctorId);
        }
    }
}

