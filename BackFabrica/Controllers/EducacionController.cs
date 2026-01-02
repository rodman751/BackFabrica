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
            _dbContext.CurrentDb = dbName;
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
        public async Task<IActionResult> PostEstudiante([FromHeader(Name = "X-DbName")] string dbName, [FromBody] Estudiante est)
        {
            _dbContext.CurrentDb = dbName;
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
        public async Task<IActionResult> PostProfesor([FromHeader(Name = "X-DbName")] string dbName, [FromBody] Profesor pro)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.CrearProfesorAsync(pro);
            return result ? Ok("Profesor registrado") : BadRequest("Error");
        }
        #endregion

        #region Endpoints Cursos
        [HttpGet("cursos")]
        public async Task<IActionResult> GetCursos([FromHeader(Name = "X-DbName")] string dbName)
        {
            _dbContext.CurrentDb = dbName;
            var lista = await _repo.ObtenerCursosAsync();
            return Ok(lista);
        }

        [HttpGet("cursos/{id}")]
        public async Task<IActionResult> GetCursoPorId([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var curso = await _repo.ObtenerCursoPorIdAsync(id);
            return curso != null ? Ok(curso) : NotFound("Curso no encontrado");
        }

        [HttpPost("cursos")]
        public async Task<IActionResult> PostCurso([FromHeader(Name = "X-DbName")] string dbName, [FromBody] Curso curso)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.CrearCursoAsync(curso);
            return result ? Ok("Curso creado") : BadRequest("Error");
        }

        [HttpPut("cursos/{id}")]
        public async Task<IActionResult> PutCurso([FromHeader(Name = "X-DbName")] string dbName, int id, [FromBody] Curso curso)
        {
            _dbContext.CurrentDb = dbName;
            curso.Id = id; // Asegurarse de que el ID del objeto coincida con el ID de la URL
            var result = await _repo.ActualizarCursoAsync(curso);
            return result ? Ok("Curso actualizado") : BadRequest("Error al actualizar");
        }
        #endregion

        #region Endpoints Inscripciones y Notas
        // POST: api/Educacion/inscribir
        [HttpPost("inscribir")]
        public async Task<IActionResult> Inscribir([FromHeader(Name = "X-DbName")] string dbName, [FromBody] Inscripcion ins)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.InscribirEstudianteAsync(ins);
            return result ? Ok("Inscripción exitosa") : Conflict("El estudiante ya está inscrito en este curso y periodo");
        }

        // POST: api/Educacion/calificar
        [HttpPost("calificar")]
        public async Task<IActionResult> Calificar([FromHeader(Name = "X-DbName")] string dbName, [FromBody] CalificarRequest req)
        {
            _dbContext.CurrentDb = dbName;
            // req es una clase auxiliar definida abajo
            var result = await _repo.CalificarEstudianteAsync(req.InscripcionId, req.Nota);
            return result ? Ok("Calificación registrada") : BadRequest("No se pudo calificar");
        }

        // GET: api/Educacion/historial/{estudianteId}
        [HttpGet("historial/{estudianteId}")]
        public async Task<IActionResult> GetHistorial([FromHeader(Name = "X-DbName")] string dbName, int estudianteId)
        {
            _dbContext.CurrentDb = dbName;
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