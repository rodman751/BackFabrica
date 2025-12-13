using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
namespace CapaDapper.DataService
{
    public class DbMetadataRepository : IDbMetadataRepository
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionTemplate;

        public DbMetadataRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            // Leemos la plantilla "Server=...;Database={0};..."
            _connectionTemplate = _configuration.GetConnectionString("TemplateConnection");
        }

   
        private IDbConnection CrearConexion(string baseDatos)
        {
            // Reemplazamos el {0} por el nombre de la DB que nos pasan
            string connectionString = string.Format(_connectionTemplate, baseDatos);
            return new SqlConnection(connectionString);
        }

        public async Task<IEnumerable<string>> ObtenerNombresDeBasesDeDatosAsync()
        {
            // Nos conectamos a 'master' para ver qué bases de datos existen
            using (var connection = CrearConexion("master"))
            {
                // Filtramos database_id > 4 para no traer las del sistema (master, tempdb, etc.)
                var query = "SELECT name FROM sys.databases WHERE database_id > 4 ORDER BY name";

                return await connection.QueryAsync<string>(query);
                
            }
        }

        public async Task<string> ObtenerEsquemaJsonAsync(string nombreBaseDatos)
        {
            // 1. Nos conectamos DIRECTAMENTE a la base de datos que eligió el usuario
            using (var connection = CrearConexion(nombreBaseDatos))
            {
                // 2. Ejecutamos el Store Procedure que creamos antes
                // Como el SP devuelve una sola celda con un JSON gigante, usamos QueryFirstOrDefaultAsync
                var resultado = await connection.QueryFirstOrDefaultAsync<string>(
                    "sp_GetDatabaseSchema",
                    commandType: CommandType.StoredProcedure
                );

                return resultado;
            }
        }
    }
}
