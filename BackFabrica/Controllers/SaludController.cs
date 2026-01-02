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

        [HttpGet("pacientes/{id:int}")]
        public async Task<IActionResult> GetPacientePorId([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var paciente = await _repo.ObtenerPacientePorIdAsync(id);
            return paciente != null ? Ok(paciente) : NotFound("Paciente no encontrado");
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
            pac.Id = id;
            var result = await _repo.ActualizarHistoriaClinicaAsync(pac);
            return result ? Ok("Historia clínica actualizada") : BadRequest("Error al actualizar");
        }

        [HttpDelete("pacientes/{id}")]
        public async Task<IActionResult> DeletePaciente([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.EliminarPacienteAsync(id);
            return result ? Ok("Paciente eliminado") : NotFound("Paciente no encontrado");
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

        [HttpGet("medicos/{id}")]
        public async Task<IActionResult> GetMedicoPorId([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var medico = await _repo.ObtenerMedicoPorIdAsync(id);
            return medico != null ? Ok(medico) : NotFound("Médico no encontrado");
        }

        [HttpPost("medicos")]
        public async Task<IActionResult> PostMedico([FromHeader(Name = "X-DbName")] string dbName, [FromBody] Medico med)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.CrearMedicoAsync(med);
            return result ? Ok("Médico registrado") : BadRequest("Error");
        }

        [HttpPut("medicos/{id}")]
        public async Task<IActionResult> PutMedico([FromHeader(Name = "X-DbName")] string dbName, int id, [FromBody] Medico medico)
        {
            _dbContext.CurrentDb = dbName;
            medico.Id = id;
            var result = await _repo.ActualizarMedicoAsync(medico);
            return result ? Ok("Médico actualizado") : BadRequest("Error al actualizar");
        }

        [HttpDelete("medicos/{id}")]
        public async Task<IActionResult> DeleteMedico([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.EliminarMedicoAsync(id);
            return result ? Ok("Médico eliminado") : NotFound("Médico no encontrado");
        }
        #endregion

        #region Endpoints Citas
        [HttpGet("citas")]
        public async Task<IActionResult> GetCitas([FromHeader(Name = "X-DbName")] string dbName)
        {
            _dbContext.CurrentDb = dbName;
            var lista = await _repo.ObtenerCitasAsync();
            return Ok(lista);
        }

        [HttpGet("citas/{id:int}")]
        public async Task<IActionResult> GetCitaPorId([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var cita = await _repo.ObtenerCitaPorIdAsync(id);
            return cita != null ? Ok(cita) : NotFound("Cita no encontrada");
        }

        [HttpPost("citas")]
        public async Task<IActionResult> AgendarCita([FromHeader(Name = "X-DbName")] string dbName, [FromBody] Cita cita)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.AgendarCitaAsync(cita);
            return result ? Ok("Cita agendada") : BadRequest("Error al agendar");
        }

        [HttpGet("citas/agenda/{medicoId}")]
        public async Task<IActionResult> GetAgenda([FromHeader(Name = "X-DbName")] string dbName, int medicoId, [FromQuery] DateTime? fecha)
        {
            _dbContext.CurrentDb = dbName;
            // Si no envían fecha, usamos la de hoy
            var fechaBusqueda = fecha ?? DateTime.Today;
            var agenda = await _repo.ObtenerAgendaMedicoAsync(medicoId, fechaBusqueda);
            return Ok(agenda);
        }

        [HttpPut("citas/{id}")]
        public async Task<IActionResult> PutCita([FromHeader(Name = "X-DbName")] string dbName, int id, [FromBody] Cita cita)
        {
            _dbContext.CurrentDb = dbName;
            cita.Id = id;
            var result = await _repo.ActualizarCitaAsync(cita);
            return result ? Ok("Cita actualizada") : BadRequest("Error al actualizar");
        }

        [HttpDelete("citas/{id}")]
        public async Task<IActionResult> DeleteCita([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.EliminarCitaAsync(id);
            return result ? Ok("Cita eliminada") : NotFound("Cita no encontrada");
        }
        #endregion

        #region Endpoints Diagnosticos
        [HttpGet("diagnosticos")]
        public async Task<IActionResult> GetDiagnosticos([FromHeader(Name = "X-DbName")] string dbName)
        {
            _dbContext.CurrentDb = dbName;
            var lista = await _repo.ObtenerDiagnosticosAsync();
            return Ok(lista);
        }

        [HttpGet("diagnosticos/{id}")]
        public async Task<IActionResult> GetDiagnosticoPorId([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var diagnostico = await _repo.ObtenerDiagnosticoPorIdAsync(id);
            return diagnostico != null ? Ok(diagnostico) : NotFound("Diagnóstico no encontrado");
        }

        [HttpPost("diagnosticos")]
        public async Task<IActionResult> RegistrarDiagnostico([FromHeader(Name = "X-DbName")] string dbName, [FromBody] Diagnostico diag)
        {
            _dbContext.CurrentDb = dbName;
            // Al crear el diagnóstico, la cita se cerrará automáticamente en BD
            var result = await _repo.RegistrarDiagnosticoAsync(diag);
            return result ? Ok("Diagnóstico registrado y cita completada") : BadRequest("Error al procesar");
        }

        [HttpPut("diagnosticos/{id}")]
        public async Task<IActionResult> PutDiagnostico([FromHeader(Name = "X-DbName")] string dbName, int id, [FromBody] Diagnostico diagnostico)
        {
            _dbContext.CurrentDb = dbName;
            diagnostico.Id = id;
            var result = await _repo.ActualizarDiagnosticoAsync(diagnostico);
            return result ? Ok("Diagnóstico actualizado") : BadRequest("Error al actualizar");
        }

        [HttpDelete("diagnosticos/{id}")]
        public async Task<IActionResult> DeleteDiagnostico([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.EliminarDiagnosticoAsync(id);
            return result ? Ok("Diagnóstico eliminado") : NotFound("Diagnóstico no encontrado");
        }
        #endregion
    }
}