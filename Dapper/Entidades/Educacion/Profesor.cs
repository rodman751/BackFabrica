namespace CapaDapper.Entidades.Educacion
{
    /// <summary>
    /// Represents a teacher in the education module.
    /// </summary>
    public class Profesor
    {
        /// <summary>Unique identifier of the teacher record.</summary>
        public int Id { get; set; }
        /// <summary>Optional foreign key linking the teacher to a system user account.</summary>
        public int? UsuarioId { get; set; }
        /// <summary>Full name of the teacher.</summary>
        public string Nombres { get; set; }
        /// <summary>Teaching specialization or subject area.</summary>
        public string Especialidad { get; set; }
        /// <summary>Contact email address.</summary>
        public string Email { get; set; }
    }
}
