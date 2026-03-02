namespace CapaDapper.Entidades.Educacion
{
    /// <summary>
    /// Represents a student enrolled in the education module.
    /// </summary>
    public class Estudiante
    {
        /// <summary>Unique identifier of the student record.</summary>
        public int Id { get; set; }
        /// <summary>Optional foreign key linking the student to a system user account.</summary>
        public int? Usuario_Id { get; set; }
        /// <summary>National identity document number (cédula).</summary>
        public string Cedula { get; set; }
        /// <summary>First and middle names of the student.</summary>
        public string Nombres { get; set; }
        /// <summary>Last names of the student.</summary>
        public string Apellidos { get; set; }
        /// <summary>Date of birth.</summary>
        public DateTime Fecha_Nacimiento { get; set; }
        /// <summary>Indicates whether the student record is active.</summary>
        public bool Activo { get; set; }
        /// <summary>Timestamp when the record was created.</summary>
        public DateTime Created_At { get; set; }
    }
}
