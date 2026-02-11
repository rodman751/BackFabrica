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
		/// Obtiene la cadena de conexión del perfil seleccionado en la sesión
		/// Si no hay perfil seleccionado, usa TemplateConnection por defecto
		/// </summary>
		private string ObtenerConnectionTemplate()
		{
			try
			{
				var httpContext = _httpContextAccessor.HttpContext;

				// 1. PRIORIDAD ALTA: Leer perfil desde el header X-Connection-Profile (para API/APK)
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

				// 2. PRIORIDAD MEDIA: Leer perfil desde la sesión (para MVC)
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
				Console.WriteLine($"Error al leer perfil de conexión: {ex.Message}");
			}

			// 3. Fallback: usar TemplateConnection por defecto
			return _configuration.GetConnectionString("TemplateConnection");
		}

		public string CreateConnectionString(string dbName)
		{
			var template = ObtenerConnectionTemplate();
			return string.Format(template, dbName);
		}

		public IDbConnection CreateConnection(string dbName) =>
			new SqlConnection(CreateConnectionString(dbName));

		#region CConfig conexion
		private IDbConnection CrearConexion(string baseDatos)
		{
			var template = ObtenerConnectionTemplate();
			string connectionString = string.Format(template, baseDatos);
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
				Console.WriteLine($"Error en CrearNuevoModuloAsync: {ex.Message}");
				return false;
			}
		}

        #region mapear a json para crear db
        public async Task<string> ParseSqlToSchemaJson(string fileContent, string dbName)
        {
            if (string.IsNullOrWhiteSpace(fileContent))
                throw new ArgumentException("El contenido está vacío.");

            // 1. Limpieza inicial
            string cleanContent = fileContent.Replace("\r\n", "\n");
            cleanContent = Regex.Replace(cleanContent, @"/\*[\s\S]*?\*/", string.Empty, RegexOptions.Singleline);
            cleanContent = Regex.Replace(cleanContent, @"--.*?$", string.Empty, RegexOptions.Multiline);
            cleanContent = cleanContent.Replace("[", "").Replace("]", "");

            // 2. Detección de tablas
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

                    // Regex mejorado para capturar Nombre Tipo(Largo) Resto
                    // Ejemplo: [Precio] [DECIMAL](10,2) [NOT NULL]
                    var colRegex = new Regex(@"^(\w+)\s+([\w\(\),]+)(.*)$", RegexOptions.IgnoreCase);
                    var colMatch = colRegex.Match(line);

                    if (colMatch.Success)
                    {
                        string colName = colMatch.Groups[1].Value;
                        string colType = colMatch.Groups[2].Value;
                        string extra = colMatch.Groups[3].Value.Trim().TrimEnd(',');

                        // Saltamos columnas que el SP ya crea por defecto
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