using Microsoft.AspNetCore.Mvc;
using ClinicaMedica.Database;
using ClinicaMedica.Models;

namespace ClinicaMedica.Controllers
{
    [ApiController]
    [Route("api/clinica/[controller]")]
    public class DoctorsController : ControllerBase
    {
        private readonly DoctorRepository _doctorRepo;

        public DoctorsController(DoctorRepository doctorRepo)
        {
            _doctorRepo = doctorRepo;
        }

        // RETO 1: Catálogo de especialidades médicas
        // PREGUNTA DE SEGURIDAD:
        // ¿Un usuario no registrado (visitante) debe poder ver las especialidades y costos antes de registrarse?
        // ¿Debe llevar algún atributo de autorización o debe ser público? Justifica tu respuesta.
        [HttpGet("specialties")]
        public async Task<GeneralResponse<List<Specialty>>> GetSpecialties()
        {
            // 1. Consultar las especialidades activas desde _doctorRepo.GetActiveSpecialtiesAsync().
            // 2. Retornar la lista envuelta en un GeneralResponse<List<Specialty>> con Success = true.

            throw new NotImplementedException("El alumno debe implementar la consulta del catálogo de especialidades.");
        }

        // RETO 2: Directorio de médicos y horarios disponibles
        // PREGUNTA DE SEGURIDAD:
        // ¿Este endpoint requiere token o debe ser público?
        // ¿Cómo se recibe el filtro opcional de especialidad desde la URL (FromQuery / FromRoute)?
        [HttpGet]
        public async Task<GeneralResponse<List<Doctor>>> GetDoctors([FromQuery] int? specialtyId)
        {
            // 1. Consultar los médicos activos desde _doctorRepo.GetActiveDoctorsAsync(specialtyId).
            // 2. Retornar los resultados envueltos en un GeneralResponse<List<Doctor>> con Success = true.

            throw new NotImplementedException("El alumno debe implementar el directorio de médicos con filtro opcional.");
        }
    }
}
