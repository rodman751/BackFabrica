using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.Marshalling.IIUnknownCacheStrategy;

namespace CapaDapper.Dtos
{
    public class DbSchema
    {
        public List<TableInfo> Tables { get; set; }
        public List<ColumnInfo> Columns { get; set; }
        public List<PkInfo> Pk_Info { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("database_name")]
        public string DatabaseName { get; set; }
    }
}
