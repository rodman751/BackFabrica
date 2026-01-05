namespace CapaDapper.Entidades.Educacion
{
    public class Estudiante
    {
        public int Id { get; set; }
        public int? Usuario_Id { get; set; } 
        public string Cedula { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public DateTime Fecha_Nacimiento { get; set; }
        public bool Activo { get; set; } 
        public DateTime Created_At { get; set; }
    }
}