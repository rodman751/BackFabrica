namespace CapaDapper.Entidades.Salud
{
    public class Diagnostico
    {
        public int Id { get; set; }
        public int? Cita_Id { get; set; } // FK, relación 1 a 1
        public string Descripcion_Diagnostico { get; set; }
        public string Tratamiento_Recetado { get; set; }
        public DateTime? Proxima_Visita { get; set; }
    }
}   