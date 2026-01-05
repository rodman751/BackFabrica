namespace CapaDapper.Entidades.Salud
{
    public class Cita
    {
        public int Id { get; set; }
        public int? paciente_id { get; set; } // FK
        public int? medico_id { get; set; }   // FK
        public DateTime fecha_hora { get; set; }
        public string motivo_consulta { get; set; }
        public string Estado { get; set; } 
        public DateTime Created_At { get; set; }
    }
}