using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDapper.Dtos
{
    /// <summary>
    /// Describes a single column within a database table, as returned by <c>sp_GetDatabaseSchema</c>.
    /// Used by <c>DynamicCrudService</c> to filter and map field values during INSERT and UPDATE operations.
    /// </summary>
    public class ColumnInfo
    {
        /// <summary>Name of the table this column belongs to.</summary>
        public string Table { get; set; }
        /// <summary>Column name as defined in the database.</summary>
        public string Name { get; set; }
        /// <summary>SQL data type of the column (e.g., <c>int</c>, <c>nvarchar</c>).</summary>
        public string Type { get; set; }
        /// <summary>
        /// Indicates whether this column is an auto-increment identity column.
        /// Identity columns are excluded from INSERT statements.
        /// </summary>
        public bool Is_Identity { get; set; }
    }
}
