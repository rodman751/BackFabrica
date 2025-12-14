using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDapper.Entidades.Productos
{
    public class categorias
    {
        public int Id { get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public string padre_id { get; set; }
        public bool activo { get; set; }


    }
}
