using Microsoft.AspNetCore.Mvc;
using ClinicaMedica.Models;
using ClinicaMedica.Services;

namespace ClinicaMedica.Controllers
{
    [ApiController]
    [Route("api/clinica/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly AppointmentService _appointmentService;

        public AppointmentsController(AppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        // RETO 1: Agendar una cita médica
        // PREGUNTA DE SEGURIDAD: 
        // ¿Quién debe poder agendar una cita? ¿Es público? ¿Requiere Token? ¿Qué roles ("Patient", "Doctor", "Admin")?
        [HttpPost]
        public async Task<GeneralResponse<int>> CreateAppointment([FromBody] CreateAppointmentRequest request)
        {
            // Conectar con el servicio correspondiente
            return await _appointmentService.CreateAppointmentAsync(request);
        }

        // RETO 2: Consultar "Mis Citas"
        // PREGUNTA DE SEGURIDAD:
        // ¿Cualquier usuario puede ver las citas de otros? ¿Qué decorador asegura que solo el paciente vea su historial?
        [HttpGet("my-appointments")]
        public async Task<GeneralResponse<List<AppointmentResponse>>> GetMyAppointments()
        {
            // Conectar con el servicio correspondiente
            return await _appointmentService.GetMyAppointmentsAsync();
        }

        // RETO 3: Consultar agenda médica del personal clínico
        // PREGUNTA DE SEGURIDAD:
        // ¿Un paciente común debería poder ver la agenda de todos los consultorios? ¿Qué roles deben permitirse?
        [HttpGet("agenda")]
        public async Task<GeneralResponse<List<AppointmentResponse>>> GetStaffAgenda([FromQuery] DateTime? date)
        {
            // Conectar con el servicio correspondiente
            return await _appointmentService.GetStaffAgendaAsync(date);
        }

        // RETO 4: Cancelar una cita médica
        // PREGUNTA DE SEGURIDAD:
        // ¿Qué atributo permite acceder a cualquier usuario autenticado, dejando que el servicio valide las reglas de negocio?
        [HttpPost("{id}/cancel")]
        public async Task<GeneralResponse<bool>> CancelAppointment([FromRoute] int id, [FromQuery] string? reason)
        {
            // Conectar con el servicio correspondiente
            return await _appointmentService.CancelAppointmentAsync(id, reason);
        }

        // RETO 5: Integración con Bot de WhatsApp / SMS (Consulta de recordatorios)
        // PREGUNTA DE SEGURIDAD:
        // Este endpoint es consumido por un servicio externo automatizado en segundo plano (no hay un usuario humano logueado).
        // ¿Qué mecanismo de autenticación debe utilizarse aquí?
        [HttpGet("external/pending-reminders")]
        public async Task<GeneralResponse<List<ReminderAppointmentResponse>>> GetPendingReminders([FromQuery] DateTime? date)
        {
            var target = date ?? DateTime.Today.AddDays(1);
            return await _appointmentService.GetUpcomingRemindersAsync(target);
        }

        // RETO 6: Webhook de confirmación de recordatorio enviado
        // PREGUNTA DE SEGURIDAD:
        // ¿Qué decorador garantiza que solo el bot de mensajería externo pueda reportar el estado de envío?
        [HttpPatch("external/{id}/reminder-sent")]
        public async Task<GeneralResponse<bool>> MarkReminderSent([FromRoute] int id)
        {
            // Conectar con el servicio correspondiente
            return await _appointmentService.ConfirmReminderSentAsync(id);
        }
    }
}
