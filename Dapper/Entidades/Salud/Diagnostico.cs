namespace CapaDapper.Entidades.Salud
{
    public class Diagnostico
    {
        public int Id { get; set; }
        public int? CitaId { get; set; } // FK, relación 1 a 1
        public string DescripcionDiagnostico { get; set; }
        public string TratamientoRecetado { get; set; }
        public DateTime? ProximaVisita { get; set; }
    }
}   