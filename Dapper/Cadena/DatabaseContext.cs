using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDapper.Cadena
{
    public class DatabaseContext : IDatabaseContext
    {
        public string CurrentDb { get; set; }
    }
}
