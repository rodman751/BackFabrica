namespace CapaDapper.Entidades.Educacion
{
    public class Inscripcion
    {
        public int Id { get; set; }
        public int EstudianteId { get; set; } // FK
        public int CursoId { get; set; }      // FK
        public string Periodo { get; set; }   // Ej: 2025-1
        public decimal? Calificacion { get; set; } 
        public DateTime FechaInscripcion { get; set; }
    }
}