namespace ClinicaMedica.Models
{
    public class Specialty
    {
        public int SpecialtyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal ConsultationFee { get; set; }
    }

    public class Doctor
    {
        public int DoctorId { get; set; }
        public int UserId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public int SpecialtyId { get; set; }
        public string SpecialtyName { get; set; } = string.Empty;
        public string ProfessionalLicense { get; set; } = string.Empty;
        public string ConsultationRoom { get; set; } = string.Empty;
        public TimeSpan WorkStartTime { get; set; }
        public TimeSpan WorkEndTime { get; set; }
        public int SlotDurationMinutes { get; set; }
        public decimal ConsultationFee { get; set; }
    }
}

