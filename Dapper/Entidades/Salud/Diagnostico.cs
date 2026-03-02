namespace CapaDapper.Entidades.Salud
{
    /// <summary>
    /// Represents the clinical outcome of a medical appointment,
    /// recording the diagnosis and prescribed treatment.
    /// Has a one-to-one relationship with <see cref="Cita"/>.
    /// </summary>
    public class Diagnostico
    {
        /// <summary>Unique identifier of the diagnosis record.</summary>
        public int Id { get; set; }
        /// <summary>Foreign key referencing the associated appointment.</summary>
        public int? Cita_Id { get; set; }
        /// <summary>Clinical diagnosis description.</summary>
        public string Descripcion_Diagnostico { get; set; }
        /// <summary>Prescribed treatment plan or medication.</summary>
        public string Tratamiento_Recetado { get; set; }
        /// <summary>Recommended date for the patient's next visit, or <c>null</c> if not scheduled.</summary>
        public DateTime? Proxima_Visita { get; set; }
    }
}
