using CapaDapper.DataService;
using GenAPK.Models;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.Text;

namespace GenAPK.Controllers
{
    /// <summary>
    /// Orchestrates APK generation for the GenAPK MVC application.
    /// Retrieves database schemas, triggers the Flutter build pipeline,
    /// and exposes download endpoints for the compiled APK and source code archives.
    /// </summary>
    public class GeneradorController : Controller
    {
        private readonly IDbMetadataRepository _repository;
        private readonly ApkBuilderService _apkService;

        public GeneradorController(IDbMetadataRepository repository, ApkBuilderService apkService)
        {
            _repository = repository;
            _apkService = apkService;
        }

        /// <summary>
        /// Displays the APK generator view, populated with available databases
        /// from the currently selected connection profile.
        /// Redirects to the login view when no connection profile has been selected.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var profileKeyBytes = HttpContext.Session.Get("SelectedProfile");
                if (profileKeyBytes == null || profileKeyBytes.Length == 0)
                {
                    TempData["Warning"] = "Por favor, selecciona un servidor de conexión primero.";
                    return RedirectToAction("Index", "Login");
                }

                var profileKey = Encoding.UTF8.GetString(profileKeyBytes);
                ViewBag.SelectedProfile = profileKey;

                var dbs = await _repository.ObtenerNombresDeBasesDeDatosAsync();
                ViewBag.Databases = dbs;
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar bases de datos: " + ex.Message;
                return View();
            }
        }

        /// <summary>
        /// Returns the list of available databases for the active connection profile as JSON.
        /// Returns an error payload when no profile is stored in the session.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDatabases()
        {
            try
            {
                var profileKeyBytes = HttpContext.Session.Get("SelectedProfile");
                if (profileKeyBytes == null || profileKeyBytes.Length == 0)
                {
                    return Json(new { success = false, error = "No hay servidor seleccionado" });
                }

                var dbs = await _repository.ObtenerNombresDeBasesDeDatosAsync();
                return Json(new { success = true, databases = dbs });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Returns the JSON schema for the specified database.
        /// </summary>
        /// <param name="selectedDb">Name of the target database.</param>
        [HttpGet]
        public async Task<IActionResult> GetJson(string selectedDb)
        {
            try
            {
                if (string.IsNullOrEmpty(selectedDb))
                    return BadRequest("Debes especificar una base de datos (?selectedDb=Nombre)");

                var jsonSchema = await _repository.ObtenerEsquemaJsonAsync(selectedDb);
                return Content(jsonSchema, "application/json");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Generates a release APK for the specified database by injecting its schema into
        /// the Flutter project configuration and invoking the build pipeline.
        /// On success, stores artifact paths in <c>TempData</c> and redirects to <see cref="Descargar"/>.
        /// </summary>
        /// <param name="selectedDb">Name of the target database used to configure the Flutter app.</param>
        [HttpPost]
        public async Task<IActionResult> GenerarApk(string selectedDb)
        {
            if (string.IsNullOrEmpty(selectedDb))
            {
                TempData["Error"] = "Por favor selecciona una base de datos.";
                return RedirectToAction("Index");
            }

            try
            {
                var jsonSchema = await _repository.ObtenerEsquemaJsonAsync(selectedDb);

                if (string.IsNullOrEmpty(jsonSchema))
                {
                    TempData["Error"] = "El esquema obtenido está vacío o la DB no es accesible.";
                    return RedirectToAction("Index");
                }

                var buildResult = await _apkService.GenerarApkAsync(selectedDb, jsonSchema);

                if (buildResult == null || string.IsNullOrEmpty(buildResult.ApkPath))
                {
                    TempData["Error"] = "No se pudo generar el APK o la ruta está vacía.";
                    return RedirectToAction("Index");
                }

                TempData["ApkPath"] = buildResult.ApkPath;
                TempData["ZipPath"] = buildResult.SourceCodeZipPath;
                TempData["FlutterZipPath"] = buildResult.FlutterSourceCodeZipPath;
                TempData["DbName"] = selectedDb;
                TempData["Success"] = "APK y código fuente generados correctamente.";

                return RedirectToAction("Descargar");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error en la generación: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Displays the download page for the artifacts produced by the most recent build.
        /// Redirects to the generator view when no build artifacts are available.
        /// </summary>
        public IActionResult Descargar()
        {
            if (TempData.Peek("ApkPath") == null)
            {
                TempData["Error"] = "No hay archivos disponibles para descargar.";
                return RedirectToAction("Index");
            }

            ViewBag.ApkPath = TempData.Peek("ApkPath");
            ViewBag.ZipPath = TempData.Peek("ZipPath");
            ViewBag.FlutterZipPath = TempData.Peek("FlutterZipPath");
            ViewBag.DbName = TempData.Peek("DbName");
            ViewBag.Success = TempData.Peek("Success");

            return View();
        }

        /// <summary>
        /// Streams the compiled APK file to the browser as a downloadable attachment.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DescargarApk()
        {
            string rutaApk = TempData.Peek("ApkPath")?.ToString();
            string dbName = TempData.Peek("DbName")?.ToString();

            if (string.IsNullOrEmpty(rutaApk) || !System.IO.File.Exists(rutaApk))
            {
                TempData["Error"] = "El archivo APK no está disponible.";
                return RedirectToAction("Index");
            }

            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(rutaApk);
            string nombreArchivo = $"App_{dbName}_{DateTime.Now:yyyyMMdd}.apk";

            TempData.Keep();

            return File(fileBytes, "application/vnd.android.package-archive", nombreArchivo);
        }

        /// <summary>
        /// Streams the .NET backend source code archive to the browser as a downloadable attachment.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DescargarZip()
        {
            string rutaZip = TempData.Peek("ZipPath")?.ToString();
            string dbName = TempData.Peek("DbName")?.ToString();

            if (string.IsNullOrEmpty(rutaZip) || !System.IO.File.Exists(rutaZip))
            {
                TempData["Error"] = "El archivo ZIP no está disponible.";
                return RedirectToAction("Index");
            }

            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(rutaZip);
            string nombreArchivo = Path.GetFileName(rutaZip);

            TempData.Keep();

            return File(fileBytes, "application/zip", nombreArchivo);
        }

        /// <summary>
        /// Streams the Flutter source code archive to the browser as a downloadable attachment.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DescargarFlutterZip()
        {
            string rutaZip = TempData.Peek("FlutterZipPath")?.ToString();
            string dbName = TempData.Peek("DbName")?.ToString();

            if (string.IsNullOrEmpty(rutaZip) || !System.IO.File.Exists(rutaZip))
            {
                TempData["Error"] = "El archivo ZIP de Flutter no está disponible.";
                return RedirectToAction("Index");
            }

            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(rutaZip);
            string nombreArchivo = Path.GetFileName(rutaZip);

            TempData.Keep();

            return File(fileBytes, "application/zip", nombreArchivo);
        }
    }
}
