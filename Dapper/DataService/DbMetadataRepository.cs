using CapaDapper.Dtos;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CapaDapper.DataService
{
	/// <summary>
	/// Provides database metadata operations including schema discovery, module creation,
	/// and connection string resolution. Resolves the active server connection profile from
	/// the HTTP request context to support multiple deployment targets at runtime.
	/// </summary>
	public class DbMetadataRepository : IDbMetadataRepository
	{
		private readonly IConfiguration _configuration;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public DbMetadataRepository(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
		{
			_configuration = configuration;
			_httpContextAccessor = httpContextAccessor;
		}

		/// <summary>
		/// Resolves the ADO.NET connection template string for the active connection profile.
		/// Priority: X-Connection-Profile header (API/APK) > session value (MVC) > default TemplateConnection.
		/// </summary>
		private string ObtenerConnectionTemplate()
		{
			try
			{
				var httpContext = _httpContextAccessor.HttpContext;

				if (httpContext?.Request?.Headers != null && httpContext.Request.Headers.ContainsKey("X-Connection-Profile"))
				{
					var headerValue = httpContext.Request.Headers["X-Connection-Profile"].ToString();
					if (!string.IsNullOrEmpty(headerValue))
					{
						var connectionString = _configuration[$"ConnectionProfiles:{headerValue}:ConnectionString"];
						if (!string.IsNullOrEmpty(connectionString))
						{
							return connectionString;
						}
					}
				}

				var session = httpContext?.Session;
				if (session != null)
				{
					var profileKeyBytes = session.Get("SelectedProfile");
					if (profileKeyBytes != null && profileKeyBytes.Length > 0)
					{
						var profileKey = Encoding.UTF8.GetString(profileKeyBytes);
						var connectionString = _configuration[$"ConnectionProfiles:{profileKey}:ConnectionString"];

						if (!string.IsNullOrEmpty(connectionString))
						{
							return connectionString;
						}
					}
				}
			}
			catch (Exception ex)
			{
				_ = ex;
			}

			return _configuration.GetConnectionString("TemplateConnection");
		}

		/// <summary>
		/// Builds a fully-qualified ADO.NET connection string for the specified database
		/// by substituting the database name into the resolved connection template.
		/// </summary>
		/// <param name="dbName">Name of the target database.</param>
		/// <returns>A ready-to-use connection string.</returns>
		public string CreateConnectionString(string dbName)
		{
			var template = ObtenerConnectionTemplate();
			return string.Format(template, dbName);
		}

		/// <summary>
		/// Creates and returns a new <see cref="IDbConnection"/> for the specified database.
		/// </summary>
		/// <param name="dbName">Name of the target database.</param>
		public IDbConnection CreateConnection(string dbName) =>
			new SqlConnection(CreateConnectionString(dbName));

		#region CConfig conexion
		/// <summary>
		/// Creates an unopened <see cref="IDbConnection"/> targeting the specified database
		/// using the resolved connection profile template.
		/// </summary>
		private IDbConnection CrearConexion(string baseDatos)
		{
			var template = ObtenerConnectionTemplate();
			string connectionString = string.Format(template, baseDatos);
			return new SqlConnection(connectionString);
		}

		/// <summary>
		/// Returns the names of all user databases on the server (database_id &gt; 4),
		/// ordered alphabetically. Executes against the <c>master</c> database.
		/// </summary>
		public async Task<IEnumerable<string>> ObtenerNombresDeBasesDeDatosAsync()
		{
			using (var connection = CrearConexion("master"))
			{
				var query = "SELECT name FROM sys.databases WHERE database_id > 4 ORDER BY name";
				return await connection.QueryAsync<string>(query, commandTimeout: 60);
			}
		}

		/// <summary>
		/// Executes the <c>sp_GetDatabaseSchema</c> stored procedure against the specified database
		/// and returns its full JSON schema (tables, columns, primary keys, and foreign keys).
		/// Uses a 180-second command timeout to accommodate large databases.
		/// </summary>
		/// <param name="nombreBaseDatos">Name of the database whose schema is requested.</param>
		/// <returns>A JSON string representing the complete database schema.</returns>
		public async Task<string> ObtenerEsquemaJsonAsync(string nombreBaseDatos)
		{
			using (var connection = CrearConexion(nombreBaseDatos))
			{
				var resultado = await connection.QueryFirstOrDefaultAsync<string>(
					"sp_GetDatabaseSchema",
					commandType: CommandType.StoredProcedure,
					commandTimeout: 180
				);

				return resultado;
			}
		}

		#endregion

		/// <summary>
		/// Invokes the <c>sp_Master_CrearModuloCompleto</c> stored procedure to create
		/// a new database module (schema, tables, security objects, and CRUD stored procedures).
		/// Returns <c>false</c> on failure without propagating the exception.
		/// </summary>
		/// <param name="request">Module creation request containing the database name and JSON table definitions.</param>
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
				await connection.ExecuteAsync("sp_Master_CrearModuloCompleto",
					parametros,
					commandType: System.Data.CommandType.StoredProcedure,
					commandTimeout: 180);

				return true;
			}
			catch (Exception ex)
			{
				_ = ex;
				return false;
			}
		}

        #region mapear a json para crear db
        /// <summary>
        /// Parses a SQL DDL file and converts its <c>CREATE TABLE</c> statements into the
        /// JSON format expected by <c>sp_Master_CrearModuloCompleto</c>.
        /// Strips SQL comments, removes bracket delimiters, and skips auto-generated columns
        /// (<c>id</c>, <c>usuario_creacion_id</c>, <c>fecha_registro</c>).
        /// </summary>
        /// <param name="fileContent">Raw SQL DDL content to parse.</param>
        /// <param name="dbName">Target database name (reserved for future use in the output schema).</param>
        /// <returns>A compact JSON array of table definitions with column metadata.</returns>
        public async Task<string> ParseSqlToSchemaJson(string fileContent, string dbName)
        {
            if (string.IsNullOrWhiteSpace(fileContent))
                throw new ArgumentException("El contenido está vacío.");

            string cleanContent = fileContent.Replace("\r\n", "\n");
            cleanContent = Regex.Replace(cleanContent, @"/\*[\s\S]*?\*/", string.Empty, RegexOptions.Singleline);
            cleanContent = Regex.Replace(cleanContent, @"--.*?$", string.Empty, RegexOptions.Multiline);
            cleanContent = cleanContent.Replace("[", "").Replace("]", "");

            var tableRegex = new Regex(@"CREATE\s+TABLE\s+(?:(\w+)\.)?(\w+)\s*\(([\s\S]+?)\)\s*;", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = tableRegex.Matches(cleanContent);

            var finalTables = new List<object>();

            foreach (Match match in matches)
            {
                string tableName = match.Groups[2].Value;
                string rawColumns = match.Groups[3].Value;
                var tableColumns = new List<object>();

                var lines = await SplitColumnsRespectingParentheses(rawColumns);

                foreach (var line in lines.Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l)))
                {
                    if (line.ToUpper().StartsWith("CONSTRAINT") || line.ToUpper().StartsWith("PRIMARY KEY (")) continue;

                    var colRegex = new Regex(@"^(\w+)\s+([\w\(\),]+)(.*)$", RegexOptions.IgnoreCase);
                    var colMatch = colRegex.Match(line);

                    if (colMatch.Success)
                    {
                        string colName = colMatch.Groups[1].Value;
                        string colType = colMatch.Groups[2].Value;
                        string extra = colMatch.Groups[3].Value.Trim().TrimEnd(',');

                        if (new[] { "id", "usuario_creacion_id", "fecha_registro" }.Contains(colName.ToLower()))
                            continue;

                        tableColumns.Add(new
                        {
                            campo = colName,
                            tipo = colType,
                            extra = extra
                        });
                    }
                }

                finalTables.Add(new { nombre = tableName, columnas = tableColumns });
            }

            return JsonSerializer.Serialize(finalTables, new JsonSerializerOptions { WriteIndented = false });
        }

        /// <summary>
        /// Splits a raw SQL column definition block into individual column entries,
        /// respecting nested parentheses so that type declarations such as
        /// <c>DECIMAL(10, 2)</c> are not split on their internal commas.
        /// </summary>
        /// <param name="text">Raw column definitions extracted from a <c>CREATE TABLE</c> statement.</param>
        /// <returns>A list of individual column definition strings.</returns>
        public Task<List<string>> SplitColumnsRespectingParentheses(string text)
        {
            var result = new List<string>();
            int level = 0;
            var sb = new StringBuilder();
            foreach (char c in text)
            {
                if (c == '(') level++;
                if (c == ')') level--;
                if (c == ',' && level == 0)
                {
                    result.Add(sb.ToString());
                    sb.Clear();
                }
                else sb.Append(c);
            }
            if (sb.Length > 0) result.Add(sb.ToString());
            return Task.FromResult(result);
        }
        #endregion
    }
}
