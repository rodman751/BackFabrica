using CapaDapper.DataService;
using CapaDapper.Entidades.Salud;
using Microsoft.AspNetCore.Mvc;

namespace BackFabrica.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SaludController : ControllerBase
    {
        private readonly ISaludRepository _repo;

        public SaludController(ISaludRepository repository)
        {
            _repo = repository;
        }

        #region Endpoints Pacientes
        [HttpGet("pacientes")]
        public async Task<IActionResult> GetPacientes()
        {
            var lista = await _repo.ObtenerPacientesAsync();
            return Ok(lista);
        }

        [HttpGet("pacientes/buscar/{dni}")]
        public async Task<IActionResult> GetPacientePorDni(string dni)
        {
            var pac = await _repo.ObtenerPacientePorDniAsync(dni);
            return pac != null ? Ok(pac) : NotFound("Paciente no encontrado");
        }

        [HttpPost("pacientes")]
        public async Task<IActionResult> PostPaciente([FromBody] Paciente pac)
        {
            if (pac == null) return BadRequest("Datos vacíos");
            
            // Validación simple de JSON para antecedentes
            if (string.IsNullOrEmpty(pac.Antecedentes)) pac.Antecedentes = "{}";

            var result = await _repo.CrearPacienteAsync(pac);
            return result ? StatusCode(201, "Paciente registrado") : BadRequest("Error (DNI duplicado o JSON inválido)");
        }

        [HttpPut("pacientes/{id}")]
        public async Task<IActionResult> PutPaciente(int id, [FromBody] Paciente pac)
        {
            if (id != pac.Id) return BadRequest("ID no coincide");
            var result = await _repo.ActualizarHistoriaClinicaAsync(pac);
            return result ? Ok("Historia clínica actualizada") : NotFound();
        }
        #endregion

        #region Endpoints Medicos
        [HttpGet("medicos")]
        public async Task<IActionResult> GetMedicos()
        {
            var lista = await _repo.ObtenerMedicosAsync();
            return Ok(lista);
        }

        [HttpPost("medicos")]
        public async Task<IActionResult> PostMedico([FromBody] Medico med)
        {
            var result = await _repo.CrearMedicoAsync(med);
            return result ? Ok("Médico registrado") : BadRequest("Error");
        }
        #endregion

        #region Endpoints Citas y Diagnosticos
        // POST: api/Salud/citas/agendar
        [HttpPost("citas/agendar")]
        public async Task<IActionResult> AgendarCita([FromBody] Cita cita)
        {
            var result = await _repo.AgendarCitaAsync(cita);
            return result ? Ok("Cita agendada") : BadRequest("Error al agendar");
        }

        // GET: api/Salud/citas/agenda/{medicoId}?fecha=2025-10-20
        [HttpGet("citas/agenda/{medicoId}")]
        public async Task<IActionResult> GetAgenda(int medicoId, [FromQuery] DateTime? fecha)
        {
            // Si no envían fecha, usamos la de hoy
            var fechaBusqueda = fecha ?? DateTime.Today;
            var agenda = await _repo.ObtenerAgendaMedicoAsync(medicoId, fechaBusqueda);
            return Ok(agenda);
        }

        // POST: api/Salud/diagnosticar
        [HttpPost("diagnosticar")]
        public async Task<IActionResult> RegistrarDiagnostico([FromBody] Diagnostico diag)
        {
            // Al crear el diagnóstico, la cita se cerrará automáticamente en BD
            var result = await _repo.RegistrarDiagnosticoAsync(diag);
            return result ? Ok("Diagnóstico registrado y cita completada") : BadRequest("Error al procesar");
        }
        #endregion
    }
}