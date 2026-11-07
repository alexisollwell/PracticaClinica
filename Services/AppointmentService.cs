#pragma warning disable CS1998 // Deshabilitar advertencia de falta de await mientras los alumnos implementan la lógica
using ClinicaMedica.Database;
using ClinicaMedica.Models;

namespace ClinicaMedica.Services
{
    public class AppointmentService
    {
        private readonly AppointmentRepository _appointmentRepo;
        private readonly DoctorRepository _doctorRepo;
        private readonly IUserContextService _userContext;

        public AppointmentService(
            AppointmentRepository appointmentRepo, 
            DoctorRepository doctorRepo, 
            IUserContextService userContext)
        {
            _appointmentRepo = appointmentRepo;
            _doctorRepo = doctorRepo;
            _userContext = userContext;
        }

        /// RETO 1: Agendar una nueva cita médica.
        public async Task<GeneralResponse<int>> CreateAppointmentAsync(CreateAppointmentRequest request)
        {
            // 1. Extraer el UserId del paciente autenticado desde _userContext.GetUserId() (Claims).
            //    Si es nulo, responder Success = false ("Usuario no identificado").
            // 2. Validar que la fecha (request.AppointmentDate) sea presente o futura (>= DateTime.Today).
            // 3. Validar que el día de la semana no sea domingo (DayOfWeek.Sunday).
            // 4. Validar que el motivo de la cita (request.Reason) no esté vacío y tenga al menos 5 caracteres.
            // 5. Verificar que el médico exista y esté activo usando _doctorRepo.GetDoctorByIdAsync(request.DoctorId).
            // 6. Validar que el horario solicitado (StartTime) esté dentro de la jornada del médico:
            //    - Calcular EndTime = StartTime + TimeSpan.FromMinutes(doctor.SlotDurationMinutes).
            //    - StartTime debe ser >= doctor.WorkStartTime.
            //    - EndTime debe ser <= doctor.WorkEndTime.
            // 7. Validar colisiones de horario: consultar _appointmentRepo.HasDoctorCollisionAsync(...)
            //    para evitar que el doctor tenga dos citas empalmadas en el mismo rango de tiempo.
            // 8. Si todas las validaciones pasan, llamar a _appointmentRepo.CreateAppointmentAsync(...)
            //    y retornar el ID generado con un mensaje descriptivo.

            throw new NotImplementedException("El alumno debe implementar la lógica de agendamiento de citas.");
        }

        // RETO 2: Obtener citas del paciente autenticado.
        public async Task<GeneralResponse<List<AppointmentResponse>>> GetMyAppointmentsAsync()
        {
            // 1. Extraer el UserId del paciente autenticado desde _userContext.GetUserId().
            // 2. Invocar _appointmentRepo.GetMyAppointmentsAsync(patientUserId).
            // 3. Devolver la lista dentro de GeneralResponse con Success = true.

            throw new NotImplementedException("El alumno debe implementar la consulta de citas del usuario.");
        }

        // RETO 3: Obtener la agenda médica del personal clínico.
        public async Task<GeneralResponse<List<AppointmentResponse>>> GetStaffAgendaAsync(DateTime? date)
        {
            // 1. Obtener el rol del usuario actual usando _userContext.GetRole().
            // 2. Regla de negocio:
            //    - Si el rol es "Doctor": solo debe poder ver SUS PROPIAS citas (filtrar por su UserId).
            //    - Si el rol es "Admin": puede ver las citas de todos los médicos de la clínica (sin filtro de médico).
            // 3. Invocar _appointmentRepo.GetAllAppointmentsAsync(...) enviando el filtro de doctor y la fecha opcional.
            // 4. Retornar el resultado en GeneralResponse.

            throw new NotImplementedException("El alumno debe implementar la consulta de agenda con control de roles.");
        }

        // RETO 4: Cancelar una cita médica.
        public async Task<GeneralResponse<bool>> CancelAppointmentAsync(int appointmentId, string? reason)
        {
            // 1. Obtener UserId y Role desde _userContext.
            // 2. Buscar la cita en la base de datos para verificar que exista.
            // 3. Validar que la cita no se encuentre ya en estado "Cancelled" ni "Completed".
            // 4. Regla de permisos:
            //    - Si el rol es "Patient":
            //      a) Verificar que la cita le pertenezca a él.
            //      b) Validar que la cita sea con al menos 2 horas de anticipación (no se permite cancelar de última hora).
            //    - Si el rol es "Doctor" o "Admin":
            //      Pueden cancelar la cita en cualquier momento indicando el motivo.
            // 5. Actualizar el estado a "Cancelled" en el repositorio e incluir la nota de cancelación.
            // 6. Retornar Success = true si la cancelación fue exitosa.

            throw new NotImplementedException("El alumno debe implementar la cancelación de citas con reglas de anticipación.");
        }

        // RETO 5: Integración con Bot de WhatsApp/SMS.
        public async Task<GeneralResponse<List<ReminderAppointmentResponse>>> GetUpcomingRemindersAsync(DateTime targetDate)
        {
            // 1. Invocar _appointmentRepo.GetUpcomingAppointmentsForReminderAsync(targetDate).
            // 2. Retornar las citas pendientes de recordatorio dentro de GeneralResponse.

            throw new NotImplementedException("El alumno debe implementar la obtención de recordatorios para el bot.");
        }

        // RETO 6: Webhook de confirmación de entrega de recordatorio.
        public async Task<GeneralResponse<bool>> ConfirmReminderSentAsync(int appointmentId)
        {
            // 1. Marcar el recordatorio como enviado en la base de datos llamando a _appointmentRepo.MarkReminderAsSentAsync(appointmentId).
            // 2. Retornar Success = true si se actualizó el registro.

            throw new NotImplementedException("El alumno debe implementar la confirmación de envío del recordatorio.");
        }
    }
}
