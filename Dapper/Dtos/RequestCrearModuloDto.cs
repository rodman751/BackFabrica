using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDapper.Dtos
{
    /// <summary>
    /// Carries the parameters required to provision a new database module via <c>sp_CrearNuevoModulo</c>.
    /// </summary>
    public class RequestCrearModuloDto
    {
        /// <summary>Name of the target database to be created or registered as a module.</summary>
        public string NombreDb { get; set; }
        /// <summary>JSON string describing the table schema to be applied to the new module.</summary>
        public string JsonTablas { get; set; }
    }
}
