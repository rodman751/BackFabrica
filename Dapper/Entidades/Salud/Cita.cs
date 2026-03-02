namespace CapaDapper.Entidades.Salud
{
    /// <summary>
    /// Represents a medical appointment linking a patient to a physician.
    /// Closing the appointment automatically triggers the creation of a <see cref="Diagnostico"/> record.
    /// </summary>
    public class Cita
    {
        /// <summary>Unique identifier of the appointment record.</summary>
        public int Id { get; set; }
        /// <summary>Foreign key referencing the attending patient.</summary>
        public int? paciente_id { get; set; }
        /// <summary>Foreign key referencing the assigned physician.</summary>
        public int? medico_id { get; set; }
        /// <summary>Scheduled date and time of the appointment.</summary>
        public DateTime fecha_hora { get; set; }
        /// <summary>Reason or chief complaint described by the patient.</summary>
        public string motivo_consulta { get; set; }
        /// <summary>Current status of the appointment (e.g., <c>pendiente</c>, <c>cerrada</c>).</summary>
        public string Estado { get; set; }
        /// <summary>Timestamp when the record was created.</summary>
        public DateTime Created_At { get; set; }
    }
}
