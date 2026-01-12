using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDapper.Dtos
{
    public class DynamicRequestDto
    {
        // Eliminamos ConnectionString de aquí, el front no debe saber la clave del servidor
        public DbSchema? Schema { get; set; }
        public Dictionary<string, object>? Data { get; set; }
    }
}
