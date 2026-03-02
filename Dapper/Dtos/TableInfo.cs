using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDapper.Dtos
{
    /// <summary>
    /// Represents a user table entry within a database schema, as returned by <c>sp_GetDatabaseSchema</c>.
    /// Included in <see cref="DbSchema.Tables"/> to enumerate the available tables for dynamic CRUD operations.
    /// </summary>
    public class TableInfo
    {
        /// <summary>Database schema that owns the table (e.g., <c>dbo</c>).</summary>
        public string Schema { get; set; }
        /// <summary>Table name as defined in the database.</summary>
        public string Table { get; set; }
    }
}
