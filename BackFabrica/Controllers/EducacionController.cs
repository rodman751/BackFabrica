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

        [HttpGet("estudiantes/{id:int}")]
        public async Task<IActionResult> GetEstudiantePorId([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var est = await _repo.ObtenerEstudiantePorIdAsync(id);
            return est != null ? Ok(est) : NotFound("Estudiante no encontrado");
        }

        [HttpGet("estudiantes/{cedula}")]
        public async Task<IActionResult> GetEstudiantePorCedula([FromHeader(Name = "X-DbName")] string dbName, string cedula)
        {
            _dbContext.CurrentDb = dbName;
            var est = await _repo.ObtenerEstudiantePorCedulaAsync(cedula);
            return est != null ? Ok(est) : NotFound("Estudiante no encontrado");
        }

        [HttpPost("estudiantes")]
        public async Task<IActionResult> PostEstudiante([FromHeader(Name = "X-DbName")] string dbName, [FromBody] Estudiante est)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.CrearEstudianteAsync(est);
            return result ? StatusCode(201, "Estudiante matriculado") : BadRequest("Error al crear (posible cedula duplicada)");
        }

        [HttpPut("estudiantes/{id}")]
        public async Task<IActionResult> PutEstudiante([FromHeader(Name = "X-DbName")] string dbName, int id, [FromBody] Estudiante estudiante)
        {
            _dbContext.CurrentDb = dbName;
            estudiante.Id = id;
            var result = await _repo.ActualizarEstudianteAsync(estudiante);
            return result ? Ok("Estudiante actualizado") : BadRequest("Error al actualizar");
        }

        [HttpDelete("estudiantes/{id}")]
        public async Task<IActionResult> DeleteEstudiante([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.EliminarEstudianteAsync(id);
            return result ? Ok("Estudiante eliminado") : NotFound("Estudiante no encontrado");
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

        [HttpGet("profesores/{id}")]
        public async Task<IActionResult> GetProfesorPorId([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var profesor = await _repo.ObtenerProfesorPorIdAsync(id);
            return profesor != null ? Ok(profesor) : NotFound("Profesor no encontrado");
        }

        [HttpPost("profesores")]
        public async Task<IActionResult> PostProfesor([FromHeader(Name = "X-DbName")] string dbName, [FromBody] Profesor pro)
        {
            try
            {
                _dbContext.CurrentDb = dbName;
                var result = await _repo.CrearProfesorAsync(pro);
                return result ? Ok("Profesor registrado") : BadRequest("Error al crear profesor");
            }
            catch (Exception ex)
            {
                // Return the actual error message to help debug
                return StatusCode(500, $"Error del servidor: {ex.Message}");
            }
        }

        [HttpPut("profesores/{id}")]
        public async Task<IActionResult> PutProfesor([FromHeader(Name = "X-DbName")] string dbName, int id, [FromBody] Profesor profesor)
        {
            _dbContext.CurrentDb = dbName;
            profesor.Id = id;
            var result = await _repo.ActualizarProfesorAsync(profesor);
            return result ? Ok("Profesor actualizado") : BadRequest("Error al actualizar");
        }

        [HttpDelete("profesores/{id}")]
        public async Task<IActionResult> DeleteProfesor([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.EliminarProfesorAsync(id);
            return result ? Ok("Profesor eliminado") : NotFound("Profesor no encontrado");
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

            // DEBUG: Log del objeto recibido desde el frontend
            Console.WriteLine($"🌐 PutCurso - Datos recibidos del frontend:");
            Console.WriteLine($"   URL id: {id}");
            Console.WriteLine($"   curso.Id: {curso.Id}");
            Console.WriteLine($"   curso.Codigo: {curso.Codigo}");
            Console.WriteLine($"   curso.Nombre: {curso.Nombre}");
            Console.WriteLine($"   curso.Descripcion: {curso.Descripcion}");
            Console.WriteLine($"   curso.Creditos: {curso.Creditos}");
            Console.WriteLine($"   curso.ProfesorId: {curso.ProfesorId}");

            curso.Id = id; // Asegurarse de que el ID del objeto coincida con el ID de la URL
            var result = await _repo.ActualizarCursoAsync(curso);
            return result ? Ok("Curso actualizado") : BadRequest("Error al actualizar");
        }

        [HttpDelete("cursos/{id}")]
        public async Task<IActionResult> DeleteCurso([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.EliminarCursoAsync(id);
            return result ? Ok("Curso eliminado") : NotFound("Curso no encontrado");
        }
        #endregion

        #region Endpoints Inscripciones y Notas
        // GET: api/Educacion/inscripciones
        [HttpGet("inscripciones")]
        public async Task<IActionResult> GetInscripciones([FromHeader(Name = "X-DbName")] string dbName)
        {
            _dbContext.CurrentDb = dbName;
            var lista = await _repo.ObtenerInscripcionesAsync();
            return Ok(lista);
        }

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