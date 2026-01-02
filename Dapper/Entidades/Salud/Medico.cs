namespace CapaDapper.Entidades.Salud
{
    public class Medico
    {
        public int Id { get; set; }
        public int? UsuarioId { get; set; } // FK al login
        public string Nombres { get; set; }
        public string Especialidad { get; set; }
        public string NumeroLicencia { get; set; }
        public string Consultorio { get; set; }
    }
}