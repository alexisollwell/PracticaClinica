using System.Data;
using Microsoft.Data.SqlClient;
using ClinicaMedica.Models;

namespace ClinicaMedica.Database
{
    public class AppointmentRepository
    {
        private readonly string _connectionString;

        public AppointmentRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ClinicaConnection")
                                ?? configuration.GetConnectionString("DefaultConnection")
                                ?? throw new InvalidOperationException("Cadena de conexión no encontrada.");
        }

        public async Task<bool> HasDoctorCollisionAsync(int doctorId, DateTime date, TimeSpan start, TimeSpan end)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_CheckDoctorSlotCollision", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@DoctorId", doctorId);
            command.Parameters.AddWithValue("@AppointmentDate", date.Date);
            command.Parameters.AddWithValue("@StartTime", start);
            command.Parameters.AddWithValue("@EndTime", end);

            await connection.OpenAsync();
            var collisions = Convert.ToInt32(await command.ExecuteScalarAsync() ?? 0);
            return collisions > 0;
        }

        public async Task<int> CreateAppointmentAsync(int patientUserId, int doctorId, DateTime date, TimeSpan start, TimeSpan end, string reason)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_CreateAppointment", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@PatientUserId", patientUserId);
            command.Parameters.AddWithValue("@DoctorId", doctorId);
            command.Parameters.AddWithValue("@AppointmentDate", date.Date);
            command.Parameters.AddWithValue("@StartTime", start);
            command.Parameters.AddWithValue("@EndTime", end);
            command.Parameters.AddWithValue("@Reason", reason);

            var outParam = new SqlParameter("@AppointmentId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(outParam);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            return (int)outParam.Value;
        }

        public async Task<List<AppointmentResponse>> GetMyAppointmentsAsync(int patientUserId)
        {
            var list = new List<AppointmentResponse>();
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_GetMyAppointments", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@PatientUserId", patientUserId);
            await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new AppointmentResponse
                {
                    AppointmentId = Convert.ToInt32(reader["AppointmentId"]),
                    AppointmentDate = Convert.ToDateTime(reader["AppointmentDate"]),
                    StartTime = (TimeSpan)reader["StartTime"],
                    EndTime = (TimeSpan)reader["EndTime"],
                    Reason = reader["Reason"].ToString() ?? "",
                    Notes = reader["Notes"] != DBNull.Value ? reader["Notes"].ToString() : null,
                    Status = reader["Status"].ToString() ?? "",
                    DoctorName = reader["DoctorName"].ToString() ?? "",
                    SpecialtyName = reader["SpecialtyName"].ToString() ?? "",
                    ConsultationRoom = reader["ConsultationRoom"].ToString() ?? "",
                    ConsultationFee = Convert.ToDecimal(reader["ConsultationFee"])
                });
            }

            return list;
        }

        public async Task<List<AppointmentResponse>> GetAllAppointmentsAsync(int? doctorUserId = null, DateTime? date = null)
        {
            var list = new List<AppointmentResponse>();
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_GetAllAppointments", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            if (doctorUserId.HasValue)
                command.Parameters.AddWithValue("@DoctorUserId", doctorUserId.Value);
            else
                command.Parameters.AddWithValue("@DoctorUserId", DBNull.Value);

            if (date.HasValue)
                command.Parameters.AddWithValue("@Date", date.Value.Date);
            else
                command.Parameters.AddWithValue("@Date", DBNull.Value);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new AppointmentResponse
                {
                    AppointmentId = Convert.ToInt32(reader["AppointmentId"]),
                    AppointmentDate = Convert.ToDateTime(reader["AppointmentDate"]),
                    StartTime = (TimeSpan)reader["StartTime"],
                    EndTime = (TimeSpan)reader["EndTime"],
                    Reason = reader["Reason"].ToString() ?? "",
                    Notes = reader["Notes"] != DBNull.Value ? reader["Notes"].ToString() : null,
                    Status = reader["Status"].ToString() ?? "",
                    PatientName = reader["PatientName"].ToString() ?? "",
                    PatientEmail = reader["PatientEmail"].ToString() ?? "",
                    DoctorName = reader["DoctorName"].ToString() ?? "",
                    SpecialtyName = reader["SpecialtyName"].ToString() ?? "",
                    ConsultationRoom = reader["ConsultationRoom"].ToString() ?? ""
                });
            }

            return list;
        }

        public async Task<bool> UpdateStatusAsync(int appointmentId, string newStatus, string? notes = null)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_UpdateAppointmentStatus", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@AppointmentId", appointmentId);
            command.Parameters.AddWithValue("@NewStatus", newStatus);
            command.Parameters.AddWithValue("@Notes", (object?)notes ?? DBNull.Value);

            await connection.OpenAsync();
            var rows = (int)(await command.ExecuteScalarAsync() ?? 0);
            return rows > 0;
        }

        public async Task<List<ReminderAppointmentResponse>> GetUpcomingAppointmentsForReminderAsync(DateTime targetDate)
        {
            var list = new List<ReminderAppointmentResponse>();
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_GetUpcomingAppointmentsForReminder", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@TargetDate", targetDate.Date);
            await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new ReminderAppointmentResponse
                {
                    AppointmentId = Convert.ToInt32(reader["AppointmentId"]),
                    PatientName = reader["PatientName"].ToString() ?? "",
                    PatientEmail = reader["PatientEmail"].ToString() ?? "",
                    DoctorName = reader["DoctorName"].ToString() ?? "",
                    SpecialtyName = reader["SpecialtyName"].ToString() ?? "",
                    ConsultationRoom = reader["ConsultationRoom"].ToString() ?? "",
                    AppointmentDate = Convert.ToDateTime(reader["AppointmentDate"]),
                    StartTime = (TimeSpan)reader["StartTime"]
                });
            }

            return list;
        }

        public async Task<bool> MarkReminderAsSentAsync(int appointmentId)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_MarkReminderAsSent", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@AppointmentId", appointmentId);
            await connection.OpenAsync();
            var rows = (int)(await command.ExecuteScalarAsync() ?? 0);
            return rows > 0;
        }
    }
}

