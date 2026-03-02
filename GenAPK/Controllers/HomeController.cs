using CapaDapper.DataService;
using CapaDapper.Dtos;
using GenAPK.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace GenAPK.Controllers
{
	/// <summary>
	/// Handles the home view and SQL-to-module import workflow for the GenAPK MVC application.
	/// Accepts a SQL schema file, parses it into the JSON format expected by the database
	/// stored procedure, and delegates module creation to <see cref="IDbMetadataRepository"/>.
	/// </summary>
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly IDbMetadataRepository _dbMetadataRepository;

		public HomeController(ILogger<HomeController> logger, IDbMetadataRepository dbMetadataRepository)
		{
			_logger = logger;
			_dbMetadataRepository = dbMetadataRepository;
		}

		/// <summary>
		/// Renders the application home page.
		/// </summary>
		public IActionResult Index()
		{
			return View();
		}

		/// <summary>
		/// Renders the privacy information page.
		/// </summary>
		public IActionResult Privacy()
		{
			return View();
		}

		/// <summary>
		/// Processes an uploaded SQL or TXT file and creates a full database module,
		/// including tables, security objects, and stored procedures.
		/// Parses the SQL content into the JSON schema format expected by the backend
		/// stored procedure before delegating module creation.
		/// </summary>
		/// <param name="archivoSql">The uploaded SQL or TXT file containing the schema definition.</param>
		/// <param name="nombreDb">Name to assign to the new database module.</param>
		[HttpPost]
		public async Task<IActionResult> ImportarDb(IFormFile archivoSql, string nombreDb)
		{
			try
			{
				if (archivoSql == null || archivoSql.Length == 0)
				{
					return Json(new { success = false, message = "No se ha seleccionado ningún archivo." });
				}

				if (string.IsNullOrWhiteSpace(nombreDb))
				{
					return Json(new { success = false, message = "Debe proporcionar un nombre para la base de datos." });
				}

				var extension = Path.GetExtension(archivoSql.FileName).ToLower();
				if (extension != ".sql" && extension != ".txt")
				{
					return Json(new { success = false, message = "Solo se permiten archivos .sql o .txt" });
				}

				string contenidoSql;
				using (var reader = new StreamReader(archivoSql.OpenReadStream(), Encoding.UTF8))
				{
					contenidoSql = await reader.ReadToEndAsync();
				}

				if (string.IsNullOrWhiteSpace(contenidoSql))
				{
					return Json(new { success = false, message = "El archivo está vacío." });
				}

				var request = new RequestCrearModuloDto
				{
					NombreDb = nombreDb.Trim()
				};

				string jsonTablas;
				try
				{
					jsonTablas = await _dbMetadataRepository.ParseSqlToSchemaJson(contenidoSql, request.NombreDb);
				}
				catch (Exception parseEx)
				{
					_logger.LogError(parseEx, "Error al parsear el SQL a JSON");
					return Json(new { success = false, message = $"Error al parsear el archivo SQL: {parseEx.Message}" });
				}

				request.JsonTablas = jsonTablas;

				var resultado = await _dbMetadataRepository.CrearNuevoModuloAsync(request);

				if (resultado)
				{
					return Json(new
					{
						success = true,
						message = $"Módulo '{nombreDb}' creado exitosamente con todas las tablas, seguridad y stored procedures.",
						nombreDb = nombreDb
					});
				}
				else
				{
					return Json(new { success = false, message = "Error al crear el módulo. Revise los logs del servidor." });
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al importar DB");
				return Json(new { success = false, message = $"Error: {ex.Message}" });
			}
		}

		/// <summary>
		/// Returns the error view populated with the current request identifier for diagnostic tracing.
		/// </summary>
		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
