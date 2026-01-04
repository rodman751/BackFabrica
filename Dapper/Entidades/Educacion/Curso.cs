namespace CapaDapper.Entidades.Educacion
{
    public class    Curso
    {
        public int Id { get; set; }
        public string Codigo { get; set; } // Ej: MAT-101
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int Creditos { get; set; }
        public int? Profesor_Id { get; set; } // FK
    }
}