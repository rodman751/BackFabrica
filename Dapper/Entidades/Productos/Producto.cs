using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDapper.Entidades.Productos
{
    public class Producto
    {
        public int Id { get; set; }
        public string Sku { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio_Costo { get; set; } // En SQL es DECIMAL
        public decimal Precio_Venta { get; set; }
        public int? Categoria_Id { get; set; } // int? permite nulos
        public int? Proveedor_Id { get; set; }
        public string Especificaciones { get; set; } // Dapper mapeará el JSON de SQL a este string
        public string Estado { get; set; }
        public DateTime Created_At { get; set; }
    }
}