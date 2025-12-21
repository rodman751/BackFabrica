using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDapper.DataService
{
    public interface IDbMetadataRepository
    {
        string CreateConnectionString(string dbName);
        IDbConnection CreateConnection(string dbName);
        // 1. Devuelve la lista de nombres de bases de datos disponibles
        Task<IEnumerable<string>> ObtenerNombresDeBasesDeDatosAsync();

        // 2. Se conecta a la DB específica y extrae el JSON del esquema
        Task<string> ObtenerEsquemaJsonAsync(string nombreBaseDatos);
        
    }
}
