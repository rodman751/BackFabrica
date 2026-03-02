namespace CapaDapper.Entidades.Salud
{
    /// <summary>
    /// Represents a patient in the healthcare module.
    /// </summary>
    public class Paciente
    {
        /// <summary>Unique identifier of the patient record.</summary>
        public int Id { get; set; }
        /// <summary>National identity document number (DNI), unique per patient.</summary>
        public string Dni { get; set; }
        /// <summary>First and middle names of the patient.</summary>
        public string Nombres { get; set; }
        /// <summary>Last names of the patient.</summary>
        public string Apellidos { get; set; }
        /// <summary>Date of birth.</summary>
        public DateTime Fecha_Nacimiento { get; set; }
        /// <summary>Contact phone number.</summary>
        public string Telefono { get; set; }
        /// <summary>Blood type (e.g., <c>O+</c>, <c>A-</c>).</summary>
        public string Grupo_Sanguineo { get; set; }
        /// <summary>JSON-encoded medical history and pre-existing conditions.</summary>
        public string Antecedentes { get; set; }
        /// <summary>Timestamp when the record was created.</summary>
        public DateTime Created_At { get; set; }
    }
}
