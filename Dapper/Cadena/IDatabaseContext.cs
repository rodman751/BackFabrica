using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDapper.Cadena
{
    /// <summary>
    /// Holds the name of the database currently targeted by the request.
    /// Set by each controller action before delegating to the repository layer,
    /// allowing a single <see cref="IDbConnectionFactory"/> implementation to serve
    /// multiple tenant databases without constructor injection of the database name.
    /// </summary>
    public interface IDatabaseContext
    {
        /// <summary>Name of the database to connect to for the current request.</summary>
        string CurrentDb { get; set; }
    }
}
