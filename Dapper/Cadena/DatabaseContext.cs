using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDapper.Cadena
{
    /// <summary>
    /// Scoped implementation of <see cref="IDatabaseContext"/>.
    /// Stores the active database name for the lifetime of a single HTTP request.
    /// </summary>
    public class DatabaseContext : IDatabaseContext
    {
        /// <inheritdoc/>
        public string CurrentDb { get; set; }
    }
}
