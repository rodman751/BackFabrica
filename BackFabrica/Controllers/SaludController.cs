using CapaDapper.Cadena;
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
        private readonly IDatabaseContext _dbContext;

        public SaludController(ISaludRepository repository, IDatabaseContext dbContext)
        {
            _repo = repository;
            _dbContext = dbContext;
        }

        #region Endpoints Pacientes
        [HttpGet("pacientes")]
        public async Task<IActionResult> GetPacientes([FromHeader(Name = "X-DbName")] string dbName)
        {
            _dbContext.CurrentDb = dbName;
            var lista = await _repo.ObtenerPacientesAsync();
            return Ok(lista);
        }

        [HttpGet("pacientes/buscar/{dni}")]
        public async Task<IActionResult> GetPacientePorDni([FromHeader(Name = "X-DbName")] string dbName, string dni)
        {
            _dbContext.CurrentDb = dbName;
            var pac = await _repo.ObtenerPacientePorDniAsync(dni);
            return pac != null ? Ok(pac) : NotFound("Paciente no encontrado");
        }

        [HttpPost("pacientes")]
        public async Task<IActionResult> PostPaciente([FromHeader(Name = "X-DbName")] string dbName, [FromBody] Paciente pac)
        {
            _dbContext.CurrentDb = dbName;
            if (pac == null) return BadRequest("Datos vacíos");
            
            // Validación simple de JSON para antecedentes
            if (string.IsNullOrEmpty(pac.Antecedentes)) pac.Antecedentes = "{}";

            var result = await _repo.CrearPacienteAsync(pac);
            return result ? StatusCode(201, "Paciente registrado") : BadRequest("Error (DNI duplicado o JSON inválido)");
        }

        [HttpPut("pacientes/{id}")]
        public async Task<IActionResult> PutPaciente([FromHeader(Name = "X-DbName")] string dbName, int id, [FromBody] Paciente pac)
        {
            _dbContext.CurrentDb = dbName;
            if (id != pac.Id) return BadRequest("ID no coincide");
            var result = await _repo.ActualizarHistoriaClinicaAsync(pac);
            return result ? Ok("Historia clínica actualizada") : NotFound();
        }
        #endregion

        #region Endpoints Medicos
        [HttpGet("medicos")]
        public async Task<IActionResult> GetMedicos([FromHeader(Name = "X-DbName")] string dbName)
        {
            _dbContext.CurrentDb = dbName;
            var lista = await _repo.ObtenerMedicosAsync();
            return Ok(lista);
        }

        [HttpPost("medicos")]
        public async Task<IActionResult> PostMedico([FromHeader(Name = "X-DbName")] string dbName, [FromBody] Medico med)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.CrearMedicoAsync(med);
            return result ? Ok("Médico registrado") : BadRequest("Error");
        }
        #endregion

        #region Endpoints Citas y Diagnosticos
        // POST: api/Salud/citas/agendar
        [HttpPost("citas/agendar")]
        public async Task<IActionResult> AgendarCita([FromHeader(Name = "X-DbName")] string dbName, [FromBody] Cita cita)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.AgendarCitaAsync(cita);
            return result ? Ok("Cita agendada") : BadRequest("Error al agendar");
        }

        // GET: api/Salud/citas/agenda/{medicoId}?fecha=2025-10-20
        [HttpGet("citas/agenda/{medicoId}")]
        public async Task<IActionResult> GetAgenda([FromHeader(Name = "X-DbName")] string dbName, int medicoId, [FromQuery] DateTime? fecha)
        {
            _dbContext.CurrentDb = dbName;
            // Si no envían fecha, usamos la de hoy
            var fechaBusqueda = fecha ?? DateTime.Today;
            var agenda = await _repo.ObtenerAgendaMedicoAsync(medicoId, fechaBusqueda);
            return Ok(agenda);
        }

        // POST: api/Salud/diagnosticar
        [HttpPost("diagnosticar")]
        public async Task<IActionResult> RegistrarDiagnostico([FromHeader(Name = "X-DbName")] string dbName, [FromBody] Diagnostico diag)
        {
            _dbContext.CurrentDb = dbName;
            // Al crear el diagnóstico, la cita se cerrará automáticamente en BD
            var result = await _repo.RegistrarDiagnosticoAsync(diag);
            return result ? Ok("Diagnóstico registrado y cita completada") : BadRequest("Error al procesar");
        }
        #endregion
    }
}