namespace CapaDapper.Entidades.Educacion
{
    public class Estudiante
    {
        public int Id { get; set; }
        public int? UsuarioId { get; set; } 
        public string Legajo { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public bool Activo { get; set; } 
        public DateTime CreatedAt { get; set; }
    }
}