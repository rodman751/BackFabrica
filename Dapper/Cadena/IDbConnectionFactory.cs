using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDapper.Cadena
{
    /// <summary>
    /// Defines a factory for creating ADO.NET database connections scoped to the
    /// database name stored in the current <see cref="IDatabaseContext"/>.
    /// </summary>
    public interface IDbConnectionFactory
    {
        /// <summary>
        /// Creates and returns a new <see cref="IDbConnection"/> targeting the
        /// database indicated by the current <see cref="IDatabaseContext.CurrentDb"/> value.
        /// </summary>
        IDbConnection CreateConnection();
    }
}
