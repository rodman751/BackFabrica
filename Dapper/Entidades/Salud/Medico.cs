namespace CapaDapper.Entidades.Salud
{
    /// <summary>
    /// Represents a physician in the healthcare module.
    /// </summary>
    public class Medico
    {
        /// <summary>Unique identifier of the physician record.</summary>
        public int Id { get; set; }
        /// <summary>Optional foreign key linking the physician to a system user account.</summary>
        public int? Usuario_Id { get; set; }
        /// <summary>Full name of the physician.</summary>
        public string Nombres { get; set; }
        /// <summary>Medical specialty (e.g., <c>Cardiología</c>, <c>Pediatría</c>).</summary>
        public string Especialidad { get; set; }
        /// <summary>Professional medical license number.</summary>
        public string Numero_Licencia { get; set; }
        /// <summary>Office or consulting room identifier.</summary>
        public string Consultorio { get; set; }
    }
}
