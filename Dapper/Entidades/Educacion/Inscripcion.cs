namespace CapaDapper.Entidades.Educacion
{
    public class Inscripcion
    {
        public int Id { get; set; }
        public int Estudiante_Id { get; set; } // FK
        public int Curso_Id { get; set; }      // FK
        public string Periodo { get; set; }   // Ej: 2025-1
        public decimal? Calificacion { get; set; } 
        public DateTime Fecha_Inscripcion { get; set; }
    }
}