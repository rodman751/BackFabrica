namespace CapaDapper.Entidades.Salud
{
    public class Paciente
    {
        public int Id { get; set; }
        public string Dni { get; set; } // Documento de identidad único
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public DateTime Fecha_Nacimiento { get; set; }
        public string Telefono { get; set; }
        public string Grupo_Sanguineo { get; set; } // Ej: O+, A-
        
        // Este string debe contener JSON válido.         
        public string Antecedentes { get; set; } 
        
        public DateTime Created_At { get; set; }
    }
}