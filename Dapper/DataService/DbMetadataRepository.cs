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
                throw new ArgumentException("El contenido del archivo SQL está vacío.", nameof(fileContent));

            // 1. LIMPIEZA
            string cleanContent = fileContent.Replace("\r\n", "\n");
            cleanContent = Regex.Replace(cleanContent, @"/\*[\s\S]*?\*/", string.Empty, RegexOptions.Singleline);
            cleanContent = Regex.Replace(cleanContent, @"--.*?$", string.Empty, RegexOptions.Multiline);

            // Eliminar comandos de sistema y limpieza de caracteres especiales
            cleanContent = Regex.Replace(cleanContent, @"^\s*INSERT\s+INTO.*?$", string.Empty, RegexOptions.Multiline | RegexOptions.IgnoreCase);
            cleanContent = Regex.Replace(cleanContent, @"^\s*USE\s+.*?$", string.Empty, RegexOptions.Multiline | RegexOptions.IgnoreCase);
            cleanContent = Regex.Replace(cleanContent, @"^\s*GO\s*?$", string.Empty, RegexOptions.Multiline | RegexOptions.IgnoreCase);
            cleanContent = Regex.Replace(cleanContent, @"^\s*SET\s+.*?$", string.Empty, RegexOptions.Multiline | RegexOptions.IgnoreCase);

            // Quitamos corchetes para normalizar nombres
            cleanContent = cleanContent.Replace("[", "").Replace("]", "");

            // 2. DETECCIÓN DE TABLAS
            // El Regex busca: CREATE TABLE Nombre ( contenido );
            var tableRegex = new Regex(@"CREATE\s+TABLE\s+(?:(\w+)\.)?(\w+)\s*\(([\s\S]+?)\)\s*;", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = tableRegex.Matches(cleanContent);

            if (matches.Count == 0)
                throw new InvalidOperationException("No se encontraron tablas válidas en el SQL.");

            var finalTablesForJson = new List<object>();

            foreach (Match match in matches)
            {
                string tableName = match.Groups[2].Value;
                string rawColumns = match.Groups[3].Value;

                var tableColumns = new List<object>();
                var lines = await SplitColumnsRespectingParentheses(rawColumns);

                foreach (var rawLine in lines)
                {
                    var line = rawLine.Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    var lineUpper = line.ToUpperInvariant();

                    // Saltamos constraints y primary keys manuales (el SP de SQL ya gestiona el ID y PK base)
                    if (lineUpper.StartsWith("CONSTRAINT") || (lineUpper.StartsWith("PRIMARY KEY") && line.Contains("(")))
                        continue;

                    var parts = Regex.Split(line, @"\s+");
                    if (parts.Length < 2) continue;

                    string colName = parts[0];

                    // Si el nombre de la columna es igual a los automáticos del SP, los saltamos
                    if (colName.ToLower() == "id" || colName.ToLower() == "usuario_creacion_id" || colName.ToLower() == "fecha_registro")
                        continue;

                    string colType = parts[1];

                    // Manejo de tipos con paréntesis como NVARCHAR(100)
                    if (colType.Contains("(") && !colType.Contains(")"))
                    {
                        var sb = new StringBuilder(colType);
                        for (int i = 2; i < parts.Length; i++)
                        {
                            sb.Append(parts[i]);
                            if (parts[i].Contains(")")) break;
                        }
                        colType = sb.ToString();
                    }
                    colType = colType.TrimEnd(',');

                    // Mapeo de tipos para compatibilidad
                    string upperType = colType.ToUpperInvariant();
                    if (upperType.StartsWith("INT") && lineUpper.Contains("IDENTITY")) colType = "INT";
                    else if (upperType == "TEXT") colType = "NVARCHAR(MAX)";
                    else if (upperType == "BOOL" || upperType == "BOOLEAN") colType = "BIT";

                    // Capturar el resto de la línea como "extra" (NOT NULL, DEFAULT, etc.)
                    // Quitamos el nombre y el tipo del string original para quedarnos con el resto
                    string extra = "";
                    int firstSpace = line.IndexOf(' ');
                    if (firstSpace > 0)
                    {
                        int secondSpace = line.IndexOf(' ', firstSpace + 1);
                        if (secondSpace > 0)
                        {
                            extra = line.Substring(secondSpace).Trim().TrimEnd(',');
                        }
                    }

                    tableColumns.Add(new
                    {
                        campo = colName,
                        tipo = colType,
                        extra = extra
                    });
                }

                finalTablesForJson.Add(new
                {
                    nombre = tableName,
                    columnas = tableColumns
                });
            }

            var options = new JsonSerializerOptions { WriteIndented = false };
            return JsonSerializer.Serialize(finalTablesForJson, options);
        }

        public Task<List<string>> SplitColumnsRespectingParentheses(string text)
        {
            var result = new List<string>();
            int parenthesisLevel = 0;
            var buffer = new StringBuilder();

            foreach (char c in text)
            {
                if (c == '(') parenthesisLevel++;
                if (c == ')') parenthesisLevel--;

                if (c == ',' && parenthesisLevel == 0)
                {
                    result.Add(buffer.ToString());
                    buffer.Clear();
                }
                else
                {
                    buffer.Append(c);
                }
            }

            if (buffer.Length > 0) result.Add(buffer.ToString());
            return Task.FromResult(result);
        }
        #endregion
    }
}