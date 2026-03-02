using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.Marshalling.IIUnknownCacheStrategy;

namespace CapaDapper.Dtos
{
    /// <summary>
    /// Represents the full metadata schema for a database as returned by <c>sp_GetDatabaseSchema</c>.
    /// Passed from the Flutter client to the dynamic CRUD endpoints to enable schema-driven operations.
    /// </summary>
    public class DbSchema
    {
        /// <summary>All user tables defined in the database.</summary>
        public List<TableInfo> Tables { get; set; }
        /// <summary>All columns across all user tables, including type and identity information.</summary>
        public List<ColumnInfo> Columns { get; set; }
        /// <summary>Primary key metadata for each table, used to build WHERE clauses.</summary>
        public List<PkInfo> Pk_Info { get; set; }
        /// <summary>Name of the source database. Deserialized from the JSON property <c>database_name</c>.</summary>
        [System.Text.Json.Serialization.JsonPropertyName("database_name")]
        public string DatabaseName { get; set; }
    }
}
