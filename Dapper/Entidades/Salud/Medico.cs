namespace CapaDapper.Entidades.Salud
{
    public class Medico
    {
        public int Id { get; set; }
        public int? Usuario_Id { get; set; } // FK al login
        public string Nombres { get; set; }
        public string Especialidad { get; set; }
        public string Numero_Licencia { get; set; }
        public string Consultorio { get; set; }
    }
}