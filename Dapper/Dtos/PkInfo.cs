using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDapper.Dtos
{
    /// <summary>
    /// Describes the primary key column for a database table, as returned by <c>sp_GetDatabaseSchema</c>.
    /// Used by <c>DynamicCrudService</c> to build parameterized WHERE clauses for UPDATE and GET-by-ID operations.
    /// </summary>
    public class PkInfo
    {
        /// <summary>Name of the table this primary key belongs to.</summary>
        public string Table { get; set; }
        /// <summary>Column name of the primary key.</summary>
        public string Column { get; set; }
    }
}
