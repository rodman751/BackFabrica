using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDapper.Entidades.Educacion
{
    public class Profesor
    {
        public int Id { get; set; }
        public int? UsuarioId { get; set; } // FK al login
        public string Nombres { get; set; }
        public string Especialidad { get; set; }
        public string Email { get; set; }
    }
}
