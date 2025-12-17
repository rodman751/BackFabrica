using CapaDapper.Cadena;
using CapaDapper.DataService;
using CapaDapper.Entidades.Educacion;
using Microsoft.AspNetCore.Mvc;

namespace BackFabrica.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EducacionController : ControllerBase
    {
        private readonly IEducacionRepository _repo;
        private readonly IDatabaseContext _dbContext;
        public EducacionController(IEducacionRepository repository, IDatabaseContext dbContext)
        {
            _repo = repository;
            _dbContext = dbContext;
        }

        #region Endpoints Estudiantes
        [HttpGet("estudiantes")]
        public async Task<IActionResult> GetEstudiantes([FromHeader(Name = "X-DbName")] string dbName)
        {
            _dbContext.CurrentDb = dbName; // OBLIGATORIO
            var lista = await _repo.ObtenerEstudiantesAsync();
            return Ok(lista);
        }

        [HttpGet("estudiantes/{legajo}")]
        public async Task<IActionResult> GetEstudiantePorLegajo([FromHeader(Name = "X-DbName")] string dbName,string legajo)
        {
            _dbContext.CurrentDb = dbName;
            var est = await _repo.ObtenerEstudiantePorLegajoAsync(legajo);
            return est != null ? Ok(est) : NotFound("Estudiante no encontrado");
        }

        [HttpPost("estudiantes")]
        public async Task<IActionResult> PostEstudiante([FromBody] Estudiante est)
        {
            var result = await _repo.CrearEstudianteAsync(est);
            return result ? StatusCode(201, "Estudiante matriculado") : BadRequest("Error al crear (posible legajo duplicado)");
        }
        #endregion

        #region Endpoints Profesores
        [HttpGet("profesores")]
        public async Task<IActionResult> GetProfesores([FromHeader(Name = "X-DbName")] string dbName)
        {
            _dbContext.CurrentDb = dbName;
            var lista = await _repo.ObtenerProfesoresAsync();
            return Ok(lista);
        }

        [HttpPost("profesores")]
        public async Task<IActionResult> PostProfesor([FromBody] Profesor pro)
        {
            var result = await _repo.CrearProfesorAsync(pro);
            return result ? Ok("Profesor registrado") : BadRequest("Error");
        }
        #endregion

        #region Endpoints Cursos
        [HttpGet("cursos")]
        public async Task<IActionResult> GetCursos()
        {
            var lista = await _repo.ObtenerCursosAsync();
            return Ok(lista);
        }

        [HttpPost("cursos")]
        public async Task<IActionResult> PostCurso([FromBody] Curso curso)
        {
            var result = await _repo.CrearCursoAsync(curso);
            return result ? Ok("Curso creado") : BadRequest("Error");
        }
        #endregion

        #region Endpoints Inscripciones y Notas
        // POST: api/Educacion/inscribir
        [HttpPost("inscribir")]
        public async Task<IActionResult> Inscribir([FromBody] Inscripcion ins)
        {
            var result = await _repo.InscribirEstudianteAsync(ins);
            return result ? Ok("Inscripción exitosa") : Conflict("El estudiante ya está inscrito en este curso y periodo");
        }

        // POST: api/Educacion/calificar
        [HttpPost("calificar")]
        public async Task<IActionResult> Calificar([FromBody] CalificarRequest req)
        {
            // req es una clase auxiliar definida abajo
            var result = await _repo.CalificarEstudianteAsync(req.InscripcionId, req.Nota);
            return result ? Ok("Calificación registrada") : BadRequest("No se pudo calificar");
        }

        // GET: api/Educacion/historial/{estudianteId}
        [HttpGet("historial/{estudianteId}")]
        public async Task<IActionResult> GetHistorial(int estudianteId)
        {
            var historial = await _repo.ObtenerHistorialAcademicoAsync(estudianteId);
            return Ok(historial);
        }
        #endregion
    }

    // DTO pequeño para recibir la nota
    public class CalificarRequest
    {
        public int InscripcionId { get; set; }
        public decimal Nota { get; set; }
    }
}