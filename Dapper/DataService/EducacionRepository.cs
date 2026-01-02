using CapaDapper.Cadena;
using CapaDapper.Entidades.Educacion;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace CapaDapper.DataService
{
    public class EducacionRepository : IEducacionRepository
    {
        //INYECCION NUEVA
        private readonly IDbConnectionFactory _connectionFactory;
        public EducacionRepository(IDbConnectionFactory connectionFactory)
        {

            _connectionFactory = connectionFactory;

        }


        #region Estudiantes
        public async Task<IEnumerable<Estudiante>> ObtenerEstudiantesAsync()
        {
            var sql = "SELECT * FROM estudiantes";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryAsync<Estudiante>(sql);
        }

        public async Task<Estudiante> ObtenerEstudiantePorIdAsync(int id)
        {
            var sql = "SELECT * FROM estudiantes WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<Estudiante>(sql, new { Id = id });
        }

        public async Task<Estudiante> ObtenerEstudiantePorLegajoAsync(string legajo)
        {
            var sql = "SELECT * FROM estudiantes WHERE legajo = @Legajo";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<Estudiante>(sql, new { Legajo = legajo });
        }

        public async Task<bool> CrearEstudianteAsync(Estudiante e)
        {
            // No insertamos ID ni fechas automáticas
            var sql = @"
                INSERT INTO estudiantes (usuario_id, legajo, nombres, apellidos, fecha_nacimiento, activo)
                VALUES (@UsuarioId, @Legajo, @Nombres, @Apellidos, @FechaNacimiento, 1)";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, e) > 0;
        }

        public async Task<bool> ActualizarEstudianteAsync(Estudiante e)
        {
            var sql = @"
                UPDATE estudiantes 
                SET nombres = @Nombres, apellidos = @Apellidos, fecha_nacimiento = @FechaNacimiento, activo = @Activo
                WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, e) > 0;
        }
        #endregion

        #region Profesores
        public async Task<IEnumerable<Profesor>> ObtenerProfesoresAsync()
        {
            var sql = "SELECT * FROM profesores";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryAsync<Profesor>(sql);
        }

        public async Task<bool> CrearProfesorAsync(Profesor p)
        {
            var sql = "INSERT INTO profesores (usuario_id, nombres, especialidad, email) VALUES (@UsuarioId, @Nombres, @Especialidad, @Email)";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, p) > 0;
        }
        #endregion

        #region Cursos
        public async Task<IEnumerable<Curso>> ObtenerCursosAsync()
        {
            var sql = "SELECT * FROM cursos";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryAsync<Curso>(sql);
        }

        public async Task<Curso> ObtenerCursoPorIdAsync(int id)
        {
            var sql = "SELECT * FROM cursos WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<Curso>(sql, new { Id = id });
        }

        public async Task<bool> CrearCursoAsync(Curso c)
        {
            var sql = "INSERT INTO cursos (codigo, nombre, descripcion, creditos, profesor_id) VALUES (@Codigo, @Nombre, @Descripcion, @Creditos, @ProfesorId)";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, c) > 0;
        }

        public async Task<bool> ActualizarCursoAsync(Curso c)
        {
            var sql = @"
                UPDATE cursos
                SET codigo = @Codigo, nombre = @Nombre, descripcion = @Descripcion, creditos = @Creditos, profesor_id = @ProfesorId
                WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, c) > 0;
        }
        #endregion

        #region Inscripciones
        public async Task<bool> InscribirEstudianteAsync(Inscripcion i)
        {
            // Al inscribir, la calificación empieza como NULL
            var sql = @"
                INSERT INTO inscripciones (estudiante_id, curso_id, periodo, calificacion) 
                VALUES (@EstudianteId, @CursoId, @Periodo, NULL)";
            using var conn = _connectionFactory.CreateConnection();
            try {
                return await conn.ExecuteAsync(sql, i) > 0;
            } catch {
                return false; // Probablemente estudiante ya inscrito en ese curso/periodo
            }
        }

        public async Task<bool> CalificarEstudianteAsync(int inscripcionId, decimal nota)
        {
            var sql = "UPDATE inscripciones SET calificacion = @Nota WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, new { Id = inscripcionId, Nota = nota }) > 0;
        }

        public async Task<IEnumerable<dynamic>> ObtenerHistorialAcademicoAsync(int estudianteId)
        {
            // Hacemos JOIN para mostrar nombres reales en lugar de IDs
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