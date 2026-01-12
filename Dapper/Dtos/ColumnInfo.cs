using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDapper.Dtos
{
    public class ColumnInfo
    {
        public string Table { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public bool Is_Identity { get; set; } // Importante para no intentar insertar en IDs autoincrementables
    }
}
