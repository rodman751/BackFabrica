using CapaDapper.DataService;
using CapaDapper.Dtos;
using GenAPK.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace GenAPK.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly IDbMetadataRepository _dbMetadataRepository;

		public HomeController(ILogger<HomeController> logger, IDbMetadataRepository dbMetadataRepository)
		{
			_logger = logger;
			_dbMetadataRepository = dbMetadataRepository;
		}

		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

		/// <summary>
		/// Procesa el archivo SQL/TXT importado y crea el módulo completo
		/// </summary>
		[HttpPost]
		public async Task<IActionResult> ImportarDb(IFormFile archivoSql, string nombreDb)
		{
			try
			{
				// Validaciones
				if (archivoSql == null || archivoSql.Length == 0)
				{
					return Json(new { success = false, message = "No se ha seleccionado ningún archivo." });
				}

				if (string.IsNullOrWhiteSpace(nombreDb))
				{
					return Json(new { success = false, message = "Debe proporcionar un nombre para la base de datos." });
				}

				// Validar extensión del archivo
				var extension = Path.GetExtension(archivoSql.FileName).ToLower();
				if (extension != ".sql" && extension != ".txt")
				{
					return Json(new { success = false, message = "Solo se permiten archivos .sql o .txt" });
				}

				// Leer el contenido del archivo
				string contenidoSql;
				using (var reader = new StreamReader(archivoSql.OpenReadStream(), Encoding.UTF8))
				{
					contenidoSql = await reader.ReadToEndAsync();
				}

				if (string.IsNullOrWhiteSpace(contenidoSql))
				{
					return Json(new { success = false, message = "El archivo está vacío." });
				}

				// Crear el request para el método CrearNuevoModuloAsync
				var request = new RequestCrearModuloDto
				{
					NombreDb = nombreDb.Trim()
				};

				// Usar el parser .NET para convertir el SQL en el JSON esperado por el SP
				string jsonTablas;
				try
				{
					jsonTablas = await _dbMetadataRepository.ParseSqlToSchemaJson(contenidoSql, request.NombreDb);
				}
				catch (Exception parseEx)
				{
					_logger.LogError(parseEx, "Error al parsear el SQL a JSON");
					return Json(new { success = false, message = $"❌ Error al parsear el archivo SQL: {parseEx.Message}" });
				}

				request.JsonTablas = jsonTablas;

				// Llamar al método de creación
				var resultado = await _dbMetadataRepository.CrearNuevoModuloAsync(request);

				if (resultado)
				{
					return Json(new
					{
						success = true,
						message = $"✅ Módulo '{nombreDb}' creado exitosamente con todas las tablas, seguridad y stored procedures.",
						nombreDb = nombreDb
					});
				}
				else
				{
					return Json(new { success = false, message = "❌ Error al crear el módulo. Revise los logs del servidor." });
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al importar DB");
				return Json(new { success = false, message = $"❌ Error: {ex.Message}" });
			}
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}