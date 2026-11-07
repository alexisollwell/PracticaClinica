namespace ClinicaMedica.Models
{
    public class CreateAppointmentRequest
    {
        public int DoctorId { get; set; }
        public DateTime AppointmentDate { get; set; } // Solo fecha (ej. 2026-11-15)
        public TimeSpan StartTime { get; set; }       // Hora de inicio (ej. 10:00:00)
        public string Reason { get; set; } = string.Empty;
    }

    public class UpdateAppointmentStatusRequest
    {
        public string Status { get; set; } = string.Empty; // 'Confirmed', 'Cancelled', 'Completed'
        public string? Notes { get; set; }
    }

    public class AppointmentResponse
    {
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? PatientName { get; set; }
        public string? PatientEmail { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string SpecialtyName { get; set; } = string.Empty;
        public string ConsultationRoom { get; set; } = string.Empty;
        public decimal? ConsultationFee { get; set; }
    }

    public class ReminderAppointmentResponse
    {
        public int AppointmentId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string PatientEmail { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string SpecialtyName { get; set; } = string.Empty;
        public string ConsultationRoom { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
    }
}

