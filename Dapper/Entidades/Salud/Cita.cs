namespace CapaDapper.Entidades.Salud
{
    public class Cita
    {
        public int Id { get; set; }
        public int? PacienteId { get; set; } // FK
        public int? MedicoId { get; set; }   // FK
        public DateTime FechaHora { get; set; }
        public string MotivoConsulta { get; set; }
        public string Estado { get; set; } 
        public DateTime CreatedAt { get; set; }
    }
}