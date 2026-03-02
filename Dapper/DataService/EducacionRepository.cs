using CapaDapper.Cadena;
using CapaDapper.Entidades.Educacion;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace CapaDapper.DataService
{
    /// <summary>
    /// Implements data-access operations for the education domain, covering
    /// students, teachers, courses, enrollments, grades, and academic history.
    /// All queries execute against the database selected by <see cref="IDbConnectionFactory"/>.
    /// </summary>
    public class EducacionRepository : IEducacionRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        public EducacionRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        #region Estudiantes
        /// <summary>Returns all students.</summary>
        public async Task<IEnumerable<Estudiante>> ObtenerEstudiantesAsync()
        {
            var sql = "SELECT * FROM estudiantes";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryAsync<Estudiante>(sql);
        }

        /// <summary>Returns a single student by their identifier, or <c>null</c> when not found.</summary>
        public async Task<Estudiante> ObtenerEstudiantePorIdAsync(int id)
        {
            var sql = "SELECT * FROM estudiantes WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<Estudiante>(sql, new { Id = id });
        }

        /// <summary>Returns a single student by their national ID number (cédula), or <c>null</c> when not found.</summary>
        public async Task<Estudiante> ObtenerEstudiantePorCedulaAsync(string cedula)
        {
            var sql = "SELECT * FROM estudiantes WHERE Cedula = @Cedula";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<Estudiante>(sql, new { Cedula = cedula });
        }

        /// <summary>Inserts a new student record. Returns <c>false</c> when the cédula is already registered.</summary>
        public async Task<bool> CrearEstudianteAsync(Estudiante e)
        {
            var sql = @"
                INSERT INTO estudiantes (usuario_id, Cedula, nombres, apellidos, fecha_nacimiento, activo)
                VALUES (@Usuario_Id, @Cedula, @Nombres, @Apellidos, @Fecha_Nacimiento, 1)";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, e) > 0;
        }

        /// <summary>Updates the mutable fields of an existing student record.</summary>
        public async Task<bool> ActualizarEstudianteAsync(Estudiante e)
        {
            var sql = @"
                UPDATE estudiantes
                SET nombres = @Nombres, apellidos = @Apellidos, fecha_nacimiento = @Fecha_Nacimiento, activo = @Activo
                WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, e) > 0;
        }

        /// <summary>Deletes a student record by their identifier.</summary>
        public async Task<bool> EliminarEstudianteAsync(int id)
        {
            var sql = "DELETE FROM estudiantes WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, new { Id = id }) > 0;
        }
        #endregion

        #region Profesores
        /// <summary>Returns all teachers.</summary>
        public async Task<IEnumerable<Profesor>> ObtenerProfesoresAsync()
        {
            var sql = "SELECT * FROM profesores";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryAsync<Profesor>(sql);
        }

        /// <summary>Returns a single teacher by their identifier, or <c>null</c> when not found.</summary>
        public async Task<Profesor> ObtenerProfesorPorIdAsync(int id)
        {
            var sql = "SELECT * FROM profesores WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<Profesor>(sql, new { Id = id });
        }

        /// <summary>Inserts a new teacher record. Propagates database exceptions to the caller.</summary>
        public async Task<bool> CrearProfesorAsync(Profesor p)
        {
            try
            {
                var sql = "INSERT INTO profesores (usuario_id, nombres, especialidad, email) VALUES (@UsuarioId, @Nombres, @Especialidad, @Email)";
                using var conn = _connectionFactory.CreateConnection();
                return await conn.ExecuteAsync(sql, p) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al crear profesor: {ex.Message}", ex);
            }
        }

        /// <summary>Updates the mutable fields of an existing teacher record.</summary>
        public async Task<bool> ActualizarProfesorAsync(Profesor p)
        {
            var sql = @"
                UPDATE profesores
                SET usuario_id = @UsuarioId, nombres = @Nombres, especialidad = @Especialidad, email = @Email
                WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, p) > 0;
        }

        /// <summary>Deletes a teacher record by their identifier.</summary>
        public async Task<bool> EliminarProfesorAsync(int id)
        {
            var sql = "DELETE FROM profesores WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, new { Id = id }) > 0;
        }
        #endregion

        #region Cursos
        /// <summary>Returns all courses with explicit column aliases for Dapper mapping.</summary>
        public async Task<IEnumerable<Curso>> ObtenerCursosAsync()
        {
            var sql = @"
                SELECT
                    id AS Id,
                    codigo AS Codigo,
                    nombre AS Nombre,
                    descripcion AS Descripcion,
                    creditos AS Creditos,
                    profesor_id AS ProfesorId
                FROM cursos";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryAsync<Curso>(sql);
        }

        /// <summary>Returns a single course by its identifier, or <c>null</c> when not found.</summary>
        public async Task<Curso> ObtenerCursoPorIdAsync(int id)
        {
            var sql = @"
                SELECT
                    id AS Id,
                    codigo AS Codigo,
                    nombre AS Nombre,
                    descripcion AS Descripcion,
                    creditos AS Creditos,
                    profesor_id AS ProfesorId
                FROM cursos
                WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<Curso>(sql, new { Id = id });
        }

        /// <summary>Inserts a new course record.</summary>
        public async Task<bool> CrearCursoAsync(Curso c)
        {
            var sql = "INSERT INTO cursos (codigo, nombre, descripcion, creditos, profesor_id) VALUES (@Codigo, @Nombre, @Descripcion, @Creditos, @Profesor_Id)";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, c) > 0;
        }

        /// <summary>Updates the mutable fields of an existing course record.</summary>
        public async Task<bool> ActualizarCursoAsync(Curso c)
        {
            var sql = @"
        UPDATE cursos
        SET codigo = @Codigo, nombre = @Nombre, descripcion = @Descripcion, creditos = @Creditos, profesor_id = @Profesor_Id
        WHERE id = @Id";

            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, c) > 0;
        }

        /// <summary>Deletes a course record by its identifier.</summary>
        public async Task<bool> EliminarCursoAsync(int id)
        {
            var sql = "DELETE FROM cursos WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, new { Id = id }) > 0;
        }
        #endregion

        #region Inscripciones
        /// <summary>
        /// Returns all enrollments joined with student, course, and teacher information,
        /// ordered by enrollment date descending.
        /// </summary>
        public async Task<IEnumerable<dynamic>> ObtenerInscripcionesAsync()
        {
            var sql = @"
                SELECT
                    i.id,
                    i.estudiante_id,
                    i.curso_id,
                    i.periodo,
                    i.calificacion,
                    i.fecha_inscripcion,
                    e.Cedula AS estudiante_cedula,
                    e.nombres + ' ' + e.apellidos AS estudiante_nombre,
                    c.codigo AS curso_codigo,
                    c.nombre AS curso_nombre,
                    p.nombres AS profesor_nombre
                FROM inscripciones i
                INNER JOIN estudiantes e ON i.estudiante_id = e.id
                INNER JOIN cursos c ON i.curso_id = c.id
                LEFT JOIN profesores p ON c.profesor_id = p.id
                ORDER BY i.fecha_inscripcion DESC, i.id DESC";

            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryAsync<dynamic>(sql);
        }

        /// <summary>
        /// Enrolls a student in a course for the specified academic period.
        /// Returns <c>false</c> when a duplicate enrollment constraint is violated.
        /// </summary>
        public async Task<bool> InscribirEstudianteAsync(Inscripcion i)
        {
            var sql = @"
        INSERT INTO inscripciones (estudiante_id, curso_id, periodo, calificacion)
        VALUES (@Estudiante_Id, @Curso_Id, @Periodo, NULL)";
            using var conn = _connectionFactory.CreateConnection();
            try
            {
                return await conn.ExecuteAsync(sql, i) > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Sets the grade for an existing enrollment record.</summary>
        public async Task<bool> CalificarEstudianteAsync(int inscripcionId, decimal nota)
        {
            var sql = "UPDATE inscripciones SET calificacion = @Nota WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, new { Id = inscripcionId, Nota = nota }) > 0;
        }

        /// <summary>Removes an enrollment record by its identifier.</summary>
        public async Task<bool> EliminarInscripcionAsync(int id)
        {
            var sql = "DELETE FROM inscripciones WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, new { Id = id }) > 0;
        }

        /// <summary>
        /// Returns the complete academic history for a student,
        /// including course details, teacher, academic period, and grade, ordered by period descending.
        /// </summary>
        public async Task<IEnumerable<dynamic>> ObtenerHistorialAcademicoAsync(int estudianteId)
        {
            var sql = @"
                SELECT i.id as inscripcion_id, c.nombre as curso, c.codigo, p.nombres as profesor, i.periodo, i.calificacion
                FROM inscripciones i
                JOIN cursos c ON i.curso_id = c.id
                LEFT JOIN profesores p ON c.profesor_id = p.id
                WHERE i.estudiante_id = @Id
                ORDER BY i.periodo DESC";

            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryAsync(sql, new { Id = estudianteId });
        }
        #endregion
    }
}
