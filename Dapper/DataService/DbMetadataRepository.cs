using CapaDapper.Dtos;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System; // Agregado para Exception
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
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

		public string CreateConnectionString(string dbName) =>
		 string.Format(_connectionTemplate, dbName);

		public IDbConnection CreateConnection(string dbName) =>
			new SqlConnection(CreateConnectionString(dbName));

		#region CConfig conexion
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

				// Le damos un poco más de tiempo también al listado (60 seg)
				return await connection.QueryAsync<string>(query, commandTimeout: 60);
			}
		}

		public async Task<string> ObtenerEsquemaJsonAsync(string nombreBaseDatos)
		{
			// 1. Nos conectamos DIRECTAMENTE a la base de datos que eligió el usuario
			using (var connection = CrearConexion(nombreBaseDatos))
			{
				// 2. Ejecutamos el Store Procedure
				// SOLUCIÓN AL TIMEOUT: Agregamos commandTimeout: 180
				var resultado = await connection.QueryFirstOrDefaultAsync<string>(
					"sp_GetDatabaseSchema",
					commandType: CommandType.StoredProcedure,
					commandTimeout: 180 // <--- 3 MINUTOS DE ESPERA (CRÍTICO)
				);

				return resultado;
			}
		}

		#endregion

		public async Task<bool> CrearNuevoModuloAsync(RequestCrearModuloDto request)
		{
			using var connection = CrearConexion("master");

			var parametros = new
			{
				p_nombre_db = request.NombreDb,
				p_json_tablas_crud = request.JsonTablas
			};

			try
			{
				// Llamamos al SP Maestro
				// También aumentamos el tiempo aquí por seguridad
				await connection.ExecuteAsync("sp_Master_CrearModuloCompleto",
					parametros,
					commandType: System.Data.CommandType.StoredProcedure,
					commandTimeout: 180); // <--- 3 MINUTOS PARA CREAR TABLAS

				return true;
			}
			catch (Exception ex)
			{
				// Loguear error: ex.Message
				return false;
			}
		}
	}
}