using CapaDapper.Dtos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDapper.DataService
{
    /// <summary>
    /// Defines the contract for database metadata operations including schema discovery,
    /// module creation, connection string resolution, and SQL parsing.
    /// </summary>
    public interface IDbMetadataRepository
    {
        string CreateConnectionString(string dbName);
        IDbConnection CreateConnection(string dbName);
        Task<IEnumerable<string>> ObtenerNombresDeBasesDeDatosAsync();
        Task<string> ObtenerEsquemaJsonAsync(string nombreBaseDatos);
        Task<bool> CrearNuevoModuloAsync(RequestCrearModuloDto request);
        Task<string> ParseSqlToSchemaJson(string fileContent, string dbName);
        Task<List<string>> SplitColumnsRespectingParentheses(string text);
    }
}
