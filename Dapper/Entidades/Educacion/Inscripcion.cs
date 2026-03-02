namespace CapaDapper.Entidades.Educacion
{
    /// <summary>
    /// Represents a student enrollment in a specific course and academic period.
    /// </summary>
    public class Inscripcion
    {
        /// <summary>Unique identifier of the enrollment record.</summary>
        public int Id { get; set; }
        /// <summary>Foreign key referencing the enrolled student.</summary>
        public int Estudiante_Id { get; set; }
        /// <summary>Foreign key referencing the enrolled course.</summary>
        public int Curso_Id { get; set; }
        /// <summary>Academic period identifier (e.g., <c>2025-1</c>).</summary>
        public string Periodo { get; set; }
        /// <summary>Grade assigned to the student, or <c>null</c> if not yet graded.</summary>
        public decimal? Calificacion { get; set; }
        /// <summary>Date and time when the enrollment was registered.</summary>
        public DateTime Fecha_Inscripcion { get; set; }
    }
}
