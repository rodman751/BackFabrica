using CapaDapper.Entidades.Educacion;


namespace CapaDapper.DataService
{
    public interface IEducacionRepository
    {
        #region Estudiantes
        Task<IEnumerable<Estudiante>> ObtenerEstudiantesAsync();
        Task<Estudiante> ObtenerEstudiantePorIdAsync(int id);
        Task<Estudiante> ObtenerEstudiantePorLegajoAsync(string legajo);
        Task<bool> CrearEstudianteAsync(Estudiante estudiante);
        Task<bool> ActualizarEstudianteAsync(Estudiante estudiante);
        #endregion

        #region Profesores
        Task<IEnumerable<Profesor>> ObtenerProfesoresAsync();
        Task<bool> CrearProfesorAsync(Profesor profesor);
        #endregion

        #region Cursos
        Task<IEnumerable<Curso>> ObtenerCursosAsync();
        Task<Curso> ObtenerCursoPorIdAsync(int id);
        Task<bool> CrearCursoAsync(Curso curso);
        Task<bool> ActualizarCursoAsync(Curso curso);
        #endregion

        #region Inscripciones (Matrícula y Notas)
        Task<bool> InscribirEstudianteAsync(Inscripcion inscripcion);
        Task<bool> CalificarEstudianteAsync(int inscripcionId, decimal nota);
        Task<IEnumerable<dynamic>> ObtenerHistorialAcademicoAsync(int estudianteId);
        #endregion
    }
}