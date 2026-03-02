using CapaDapper.Cadena;
using CapaDapper.DataService;
using CapaDapper.Entidades.Salud;
using Microsoft.AspNetCore.Mvc;

namespace BackFabrica.Controllers
{
    /// <summary>
    /// Manages healthcare domain resources including patients, physicians, appointments, and diagnoses.
    /// All endpoints require the target database name via the <c>X-DbName</c> header.
    /// </summary>
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
        /// <summary>
        /// Returns all patients from the specified database.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        [HttpGet("pacientes")]
        public async Task<IActionResult> GetPacientes([FromHeader(Name = "X-DbName")] string dbName)
        {
            _dbContext.CurrentDb = dbName;
            var lista = await _repo.ObtenerPacientesAsync();
            return Ok(lista);
        }

        /// <summary>
        /// Returns a single patient by their identifier.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="id">Unique identifier of the patient.</param>
        [HttpGet("pacientes/{id:int}")]
        public async Task<IActionResult> GetPacientePorId([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var paciente = await _repo.ObtenerPacientePorIdAsync(id);
            return paciente != null ? Ok(paciente) : NotFound("Paciente no encontrado");
        }

        /// <summary>
        /// Returns a patient identified by their national ID number (DNI).
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="dni">National identification number of the patient.</param>
        [HttpGet("pacientes/buscar/{dni}")]
        public async Task<IActionResult> GetPacientePorDni([FromHeader(Name = "X-DbName")] string dbName, string dni)
        {
            _dbContext.CurrentDb = dbName;
            var pac = await _repo.ObtenerPacientePorDniAsync(dni);
            return pac != null ? Ok(pac) : NotFound("Paciente no encontrado");
        }

        /// <summary>
        /// Registers a new patient. Defaults the <c>Antecedentes</c> field to an empty JSON object
        /// when no medical history is provided.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="pac">Patient data to persist.</param>
        [HttpPost("pacientes")]
        public async Task<IActionResult> PostPaciente([FromHeader(Name = "X-DbName")] string dbName, [FromBody] Paciente pac)
        {
            _dbContext.CurrentDb = dbName;
            if (pac == null) return BadRequest("Datos vacíos");

            if (string.IsNullOrEmpty(pac.Antecedentes)) pac.Antecedentes = "{}";

            var result = await _repo.CrearPacienteAsync(pac);
            return result ? StatusCode(201, "Paciente registrado") : BadRequest("Error (DNI duplicado o JSON inválido)");
        }

        /// <summary>
        /// Updates the clinical record of an existing patient.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="id">Identifier of the patient to update.</param>
        /// <param name="pac">Updated patient data.</param>
        [HttpPut("pacientes/{id}")]
        public async Task<IActionResult> PutPaciente([FromHeader(Name = "X-DbName")] string dbName, int id, [FromBody] Paciente pac)
        {
            _dbContext.CurrentDb = dbName;
            pac.Id = id;
            var result = await _repo.ActualizarHistoriaClinicaAsync(pac);
            return result ? Ok("Historia clínica actualizada") : BadRequest("Error al actualizar");
        }

        /// <summary>
        /// Deletes a patient record by their identifier.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="id">Identifier of the patient to delete.</param>
        [HttpDelete("pacientes/{id}")]
        public async Task<IActionResult> DeletePaciente([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.EliminarPacienteAsync(id);
            return result ? Ok("Paciente eliminado") : NotFound("Paciente no encontrado");
        }
        #endregion

        #region Endpoints Medicos
        /// <summary>
        /// Returns all physicians from the specified database.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        [HttpGet("medicos")]
        public async Task<IActionResult> GetMedicos([FromHeader(Name = "X-DbName")] string dbName)
        {
            _dbContext.CurrentDb = dbName;
            var lista = await _repo.ObtenerMedicosAsync();
            return Ok(lista);
        }

        /// <summary>
        /// Returns a single physician by their identifier.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="id">Unique identifier of the physician.</param>
        [HttpGet("medicos/{id}")]
        public async Task<IActionResult> GetMedicoPorId([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var medico = await _repo.ObtenerMedicoPorIdAsync(id);
            return medico != null ? Ok(medico) : NotFound("Médico no encontrado");
        }

        /// <summary>
        /// Registers a new physician.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="med">Physician data to persist.</param>
        [HttpPost("medicos")]
        public async Task<IActionResult> PostMedico([FromHeader(Name = "X-DbName")] string dbName, [FromBody] Medico med)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.CrearMedicoAsync(med);
            return result ? Ok("Médico registrado") : BadRequest("Error");
        }

        /// <summary>
        /// Updates an existing physician record by their identifier.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="id">Identifier of the physician to update.</param>
        /// <param name="medico">Updated physician data.</param>
        [HttpPut("medicos/{id}")]
        public async Task<IActionResult> PutMedico([FromHeader(Name = "X-DbName")] string dbName, int id, [FromBody] Medico medico)
        {
            _dbContext.CurrentDb = dbName;
            medico.Id = id;
            var result = await _repo.ActualizarMedicoAsync(medico);
            return result ? Ok("Médico actualizado") : BadRequest("Error al actualizar");
        }

        /// <summary>
        /// Deletes a physician record by their identifier.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="id">Identifier of the physician to delete.</param>
        [HttpDelete("medicos/{id}")]
        public async Task<IActionResult> DeleteMedico([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.EliminarMedicoAsync(id);
            return result ? Ok("Médico eliminado") : NotFound("Médico no encontrado");
        }
        #endregion

        #region Endpoints Citas
        /// <summary>
        /// Returns all appointments from the specified database.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        [HttpGet("citas")]
        public async Task<IActionResult> GetCitas([FromHeader(Name = "X-DbName")] string dbName)
        {
            _dbContext.CurrentDb = dbName;
            var lista = await _repo.ObtenerCitasAsync();
            return Ok(lista);
        }

        /// <summary>
        /// Returns a single appointment by its identifier.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="id">Unique identifier of the appointment.</param>
        [HttpGet("citas/{id:int}")]
        public async Task<IActionResult> GetCitaPorId([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var cita = await _repo.ObtenerCitaPorIdAsync(id);
            return cita != null ? Ok(cita) : NotFound("Cita no encontrada");
        }

        /// <summary>
        /// Schedules a new appointment.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="cita">Appointment data to persist.</param>
        [HttpPost("citas")]
        public async Task<IActionResult> AgendarCita([FromHeader(Name = "X-DbName")] string dbName, [FromBody] Cita cita)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.AgendarCitaAsync(cita);
            return result ? Ok("Cita agendada") : BadRequest("Error al agendar");
        }

        /// <summary>
        /// Returns the appointment schedule for a physician on the specified date.
        /// Defaults to today's date when no date is provided.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="medicoId">Identifier of the physician whose schedule is requested.</param>
        /// <param name="fecha">Optional date filter; defaults to <see cref="DateTime.Today"/>.</param>
        [HttpGet("citas/agenda/{medicoId}")]
        public async Task<IActionResult> GetAgenda([FromHeader(Name = "X-DbName")] string dbName, int medicoId, [FromQuery] DateTime? fecha)
        {
            _dbContext.CurrentDb = dbName;
            var fechaBusqueda = fecha ?? DateTime.Today;
            var agenda = await _repo.ObtenerAgendaMedicoAsync(medicoId, fechaBusqueda);
            return Ok(agenda);
        }

        /// <summary>
        /// Updates an existing appointment by its identifier.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="id">Identifier of the appointment to update.</param>
        /// <param name="cita">Updated appointment data.</param>
        [HttpPut("citas/{id}")]
        public async Task<IActionResult> PutCita([FromHeader(Name = "X-DbName")] string dbName, int id, [FromBody] Cita cita)
        {
            _dbContext.CurrentDb = dbName;
            cita.Id = id;
            var result = await _repo.ActualizarCitaAsync(cita);
            return result ? Ok("Cita actualizada") : BadRequest("Error al actualizar");
        }

        /// <summary>
        /// Deletes an appointment by its identifier.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="id">Identifier of the appointment to delete.</param>
        [HttpDelete("citas/{id}")]
        public async Task<IActionResult> DeleteCita([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.EliminarCitaAsync(id);
            return result ? Ok("Cita eliminada") : NotFound("Cita no encontrada");
        }
        #endregion

        #region Endpoints Diagnosticos
        /// <summary>
        /// Returns all diagnoses from the specified database.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        [HttpGet("diagnosticos")]
        public async Task<IActionResult> GetDiagnosticos([FromHeader(Name = "X-DbName")] string dbName)
        {
            _dbContext.CurrentDb = dbName;
            var lista = await _repo.ObtenerDiagnosticosAsync();
            return Ok(lista);
        }

        /// <summary>
        /// Returns a single diagnosis by its identifier.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="id">Unique identifier of the diagnosis.</param>
        [HttpGet("diagnosticos/{id}")]
        public async Task<IActionResult> GetDiagnosticoPorId([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var diagnostico = await _repo.ObtenerDiagnosticoPorIdAsync(id);
            return diagnostico != null ? Ok(diagnostico) : NotFound("Diagnóstico no encontrado");
        }

        /// <summary>
        /// Records a new diagnosis. The associated appointment is automatically marked as completed
        /// by the database stored procedure upon successful insertion.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="diag">Diagnosis data to persist.</param>
        [HttpPost("diagnosticos")]
        public async Task<IActionResult> RegistrarDiagnostico([FromHeader(Name = "X-DbName")] string dbName, [FromBody] Diagnostico diag)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.RegistrarDiagnosticoAsync(diag);
            return result ? Ok("Diagnóstico registrado y cita completada") : BadRequest("Error al procesar");
        }

        /// <summary>
        /// Updates an existing diagnosis by its identifier.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="id">Identifier of the diagnosis to update.</param>
        /// <param name="diagnostico">Updated diagnosis data.</param>
        [HttpPut("diagnosticos/{id}")]
        public async Task<IActionResult> PutDiagnostico([FromHeader(Name = "X-DbName")] string dbName, int id, [FromBody] Diagnostico diagnostico)
        {
            _dbContext.CurrentDb = dbName;
            diagnostico.Id = id;
            var result = await _repo.ActualizarDiagnosticoAsync(diagnostico);
            return result ? Ok("Diagnóstico actualizado") : BadRequest("Error al actualizar");
        }

        /// <summary>
        /// Deletes a diagnosis by its identifier.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="id">Identifier of the diagnosis to delete.</param>
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
