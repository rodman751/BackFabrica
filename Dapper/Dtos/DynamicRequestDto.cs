using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDapper.Dtos
{
    /// <summary>
    /// Request payload for schema-driven CRUD operations.
    /// The connection string is resolved server-side from the active connection profile;
    /// the client only supplies the schema definition and the field values.
    /// </summary>
    public class DynamicRequestDto
    {
        /// <summary>Full database schema used to validate columns and resolve the target table.</summary>
        public DbSchema? Schema { get; set; }
        /// <summary>Field name / value pairs representing the row data to create or update.</summary>
        public Dictionary<string, object>? Data { get; set; }
    }
}
