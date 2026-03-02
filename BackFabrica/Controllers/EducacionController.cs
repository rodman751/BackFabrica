using CapaDapper.Cadena;
using CapaDapper.DataService;
using CapaDapper.Entidades.Educacion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackFabrica.Controllers
{
    /// <summary>
    /// Manages education domain resources including students, teachers, courses,
    /// enrollments, and academic grades. All endpoints require JWT authentication
    /// and the target database name via the <c>X-DbName</c> header.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
        /// <summary>
        /// Returns all students from the specified database.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        [HttpGet("estudiantes")]
        public async Task<IActionResult> GetEstudiantes([FromHeader(Name = "X-DbName")] string dbName)
        {
            _dbContext.CurrentDb = dbName;
            var lista = await _repo.ObtenerEstudiantesAsync();
            return Ok(lista);
        }

        /// <summary>
        /// Returns a single student by their identifier.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="id">Unique identifier of the student.</param>
        [HttpGet("estudiantes/{id:int}")]
        public async Task<IActionResult> GetEstudiantePorId([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var est = await _repo.ObtenerEstudiantePorIdAsync(id);
            return est != null ? Ok(est) : NotFound("Estudiante no encontrado");
        }

        /// <summary>
        /// Returns a student identified by their national ID number (cédula).
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="cedula">National identification number of the student.</param>
        [HttpGet("estudiantes/{cedula}")]
        public async Task<IActionResult> GetEstudiantePorCedula([FromHeader(Name = "X-DbName")] string dbName, string cedula)
        {
            _dbContext.CurrentDb = dbName;
            var est = await _repo.ObtenerEstudiantePorCedulaAsync(cedula);
            return est != null ? Ok(est) : NotFound("Estudiante no encontrado");
        }

        /// <summary>
        /// Enrolls a new student. Returns HTTP 409 when the national ID is already registered.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="est">Student data to persist.</param>
        [HttpPost("estudiantes")]
        public async Task<IActionResult> PostEstudiante([FromHeader(Name = "X-DbName")] string dbName, [FromBody] Estudiante est)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.CrearEstudianteAsync(est);
            return result ? StatusCode(201, "Estudiante matriculado") : BadRequest("Error al crear (posible cedula duplicada)");
        }

        /// <summary>
        /// Updates an existing student record by their identifier.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="id">Identifier of the student to update.</param>
        /// <param name="estudiante">Updated student data.</param>
        [HttpPut("estudiantes/{id}")]
        public async Task<IActionResult> PutEstudiante([FromHeader(Name = "X-DbName")] string dbName, int id, [FromBody] Estudiante estudiante)
        {
            _dbContext.CurrentDb = dbName;
            estudiante.Id = id;
            var result = await _repo.ActualizarEstudianteAsync(estudiante);
            return result ? Ok("Estudiante actualizado") : BadRequest("Error al actualizar");
        }

        /// <summary>
        /// Deletes a student record by their identifier.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="id">Identifier of the student to delete.</param>
        [HttpDelete("estudiantes/{id}")]
        public async Task<IActionResult> DeleteEstudiante([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.EliminarEstudianteAsync(id);
            return result ? Ok("Estudiante eliminado") : NotFound("Estudiante no encontrado");
        }
        #endregion

        #region Endpoints Profesores
        /// <summary>
        /// Returns all teachers from the specified database.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        [HttpGet("profesores")]
        public async Task<IActionResult> GetProfesores([FromHeader(Name = "X-DbName")] string dbName)
        {
            _dbContext.CurrentDb = dbName;
            var lista = await _repo.ObtenerProfesoresAsync();
            return Ok(lista);
        }

        /// <summary>
        /// Returns a single teacher by their identifier.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="id">Unique identifier of the teacher.</param>
        [HttpGet("profesores/{id}")]
        public async Task<IActionResult> GetProfesorPorId([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var profesor = await _repo.ObtenerProfesorPorIdAsync(id);
            return profesor != null ? Ok(profesor) : NotFound("Profesor no encontrado");
        }

        /// <summary>
        /// Registers a new teacher.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="pro">Teacher data to persist.</param>
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
                return StatusCode(500, $"Error del servidor: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates an existing teacher record by their identifier.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="id">Identifier of the teacher to update.</param>
        /// <param name="profesor">Updated teacher data.</param>
        [HttpPut("profesores/{id}")]
        public async Task<IActionResult> PutProfesor([FromHeader(Name = "X-DbName")] string dbName, int id, [FromBody] Profesor profesor)
        {
            _dbContext.CurrentDb = dbName;
            profesor.Id = id;
            var result = await _repo.ActualizarProfesorAsync(profesor);
            return result ? Ok("Profesor actualizado") : BadRequest("Error al actualizar");
        }

        /// <summary>
        /// Deletes a teacher record by their identifier.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="id">Identifier of the teacher to delete.</param>
        [HttpDelete("profesores/{id}")]
        public async Task<IActionResult> DeleteProfesor([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.EliminarProfesorAsync(id);
            return result ? Ok("Profesor eliminado") : NotFound("Profesor no encontrado");
        }
        #endregion

        #region Endpoints Cursos
        /// <summary>
        /// Returns all courses from the specified database.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        [HttpGet("cursos")]
        public async Task<IActionResult> GetCursos([FromHeader(Name = "X-DbName")] string dbName)
        {
            _dbContext.CurrentDb = dbName;
            var lista = await _repo.ObtenerCursosAsync();
            return Ok(lista);
        }

        /// <summary>
        /// Returns a single course by its identifier.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="id">Unique identifier of the course.</param>
        [HttpGet("cursos/{id}")]
        public async Task<IActionResult> GetCursoPorId([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var curso = await _repo.ObtenerCursoPorIdAsync(id);
            return curso != null ? Ok(curso) : NotFound("Curso no encontrado");
        }

        /// <summary>
        /// Creates a new course.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="curso">Course data to persist.</param>
        [HttpPost("cursos")]
        public async Task<IActionResult> PostCurso([FromHeader(Name = "X-DbName")] string dbName, [FromBody] Curso curso)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.CrearCursoAsync(curso);
            return result ? Ok("Curso creado") : BadRequest("Error");
        }

        /// <summary>
        /// Updates an existing course by its identifier.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="id">Identifier of the course to update.</param>
        /// <param name="curso">Updated course data.</param>
        [HttpPut("cursos/{id}")]
        public async Task<IActionResult> PutCurso([FromHeader(Name = "X-DbName")] string dbName, int id, [FromBody] Curso curso)
        {
            _dbContext.CurrentDb = dbName;

            curso.Id = id;
            var result = await _repo.ActualizarCursoAsync(curso);
            return result ? Ok("Curso actualizado") : BadRequest("Error al actualizar");
        }

        /// <summary>
        /// Deletes a course by its identifier.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="id">Identifier of the course to delete.</param>
        [HttpDelete("cursos/{id}")]
        public async Task<IActionResult> DeleteCurso([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.EliminarCursoAsync(id);
            return result ? Ok("Curso eliminado") : NotFound("Curso no encontrado");
        }
        #endregion

        #region Endpoints Inscripciones y Notas
        /// <summary>
        /// Returns all course enrollments from the specified database.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        [HttpGet("inscripciones")]
        public async Task<IActionResult> GetInscripciones([FromHeader(Name = "X-DbName")] string dbName)
        {
            _dbContext.CurrentDb = dbName;
            var lista = await _repo.ObtenerInscripcionesAsync();
            return Ok(lista);
        }

        /// <summary>
        /// Enrolls a student in a course for a given academic period.
        /// Returns HTTP 409 when the student is already enrolled in the same course and period.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="ins">Enrollment data containing student ID, course ID, and academic period.</param>
        [HttpPost("inscribir")]
        public async Task<IActionResult> Inscribir([FromHeader(Name = "X-DbName")] string dbName, [FromBody] Inscripcion ins)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.InscribirEstudianteAsync(ins);
            return result ? Ok("Inscripción exitosa") : Conflict("El estudiante ya está inscrito en este curso y periodo");
        }

        /// <summary>
        /// Records a grade for an existing enrollment.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="req">Grading request containing the enrollment ID and the grade value.</param>
        [HttpPost("calificar")]
        public async Task<IActionResult> Calificar([FromHeader(Name = "X-DbName")] string dbName, [FromBody] CalificarRequest req)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.CalificarEstudianteAsync(req.InscripcionId, req.Nota);
            return result ? Ok("Calificación registrada") : BadRequest("No se pudo calificar");
        }

        /// <summary>
        /// Returns the full academic history for a student, including courses taken and grades received.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="estudianteId">Identifier of the student whose academic history is requested.</param>
        [HttpGet("historial/{estudianteId}")]
        public async Task<IActionResult> GetHistorial([FromHeader(Name = "X-DbName")] string dbName, int estudianteId)
        {
            _dbContext.CurrentDb = dbName;
            var historial = await _repo.ObtenerHistorialAcademicoAsync(estudianteId);
            return Ok(historial);
        }

        /// <summary>
        /// Removes an enrollment record by its identifier.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="id">Identifier of the enrollment to delete.</param>
        [HttpDelete("inscripciones/{id}")]
        public async Task<IActionResult> EliminarInscripcion([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.EliminarInscripcionAsync(id);
            return result ? Ok("Inscripción eliminada") : NotFound("Inscripción no encontrada");
        }
        #endregion
    }

    /// <summary>
    /// Request payload for recording a grade against an existing enrollment.
    /// </summary>
    public class CalificarRequest
    {
        public int InscripcionId { get; set; }
        public decimal Nota { get; set; }
    }
}
