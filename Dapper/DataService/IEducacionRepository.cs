using CapaDapper.Entidades.Educacion;


namespace CapaDapper.DataService
{
    /// <summary>
    /// Defines the data-access contract for the education domain:
    /// students, teachers, courses, enrollments, grades, and academic history.
    /// </summary>
    public interface IEducacionRepository
    {
        #region Estudiantes
        Task<IEnumerable<Estudiante>> ObtenerEstudiantesAsync();
        Task<Estudiante> ObtenerEstudiantePorIdAsync(int id);
        Task<Estudiante> ObtenerEstudiantePorCedulaAsync(string cedula);
        Task<bool> CrearEstudianteAsync(Estudiante estudiante);
        Task<bool> ActualizarEstudianteAsync(Estudiante estudiante);
        Task<bool> EliminarEstudianteAsync(int id);
		Task<bool> EliminarInscripcionAsync(int id);
		#endregion

		#region Profesores
		Task<IEnumerable<Profesor>> ObtenerProfesoresAsync();
        Task<Profesor> ObtenerProfesorPorIdAsync(int id);
        Task<bool> CrearProfesorAsync(Profesor profesor);
        Task<bool> ActualizarProfesorAsync(Profesor profesor);
        Task<bool> EliminarProfesorAsync(int id);
        #endregion

        #region Cursos
        Task<IEnumerable<Curso>> ObtenerCursosAsync();
        Task<Curso> ObtenerCursoPorIdAsync(int id);
        Task<bool> CrearCursoAsync(Curso curso);
        Task<bool> ActualizarCursoAsync(Curso curso);
        Task<bool> EliminarCursoAsync(int id);
        #endregion

        #region Inscripciones (Matrícula y Notas)
        Task<IEnumerable<dynamic>> ObtenerInscripcionesAsync();
        Task<bool> InscribirEstudianteAsync(Inscripcion inscripcion);
        Task<bool> CalificarEstudianteAsync(int inscripcionId, decimal nota);
        Task<IEnumerable<dynamic>> ObtenerHistorialAcademicoAsync(int estudianteId);
        #endregion

    }
}