namespace CapaDapper.Entidades.Educacion
{
    /// <summary>
    /// Represents an academic course offered in the education module.
    /// </summary>
    public class Curso
    {
        /// <summary>Unique identifier of the course record.</summary>
        public int Id { get; set; }
        /// <summary>Course code used for identification (e.g., <c>MAT-101</c>).</summary>
        public string Codigo { get; set; }
        /// <summary>Full name of the course.</summary>
        public string Nombre { get; set; }
        /// <summary>Detailed description of the course content.</summary>
        public string Descripcion { get; set; }
        /// <summary>Number of academic credits awarded upon completion.</summary>
        public int Creditos { get; set; }
        /// <summary>Foreign key referencing the teacher assigned to the course.</summary>
        public int? Profesor_Id { get; set; }
    }
}
