using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDapper.Dtos
{
    public class ValidarLoginResult
    {
        public UsuarioLogin Usuario { get; set; }
        public string Rol { get; set; }
        public string ModuloOrigen { get; set; }
        public string Mensaje { get; set; }
        public bool EsExitoso => Mensaje?.StartsWith("SUCCESS") ?? false;
    }
}
