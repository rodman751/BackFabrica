using CapaDapper.DataService;
using GenAPK.Models;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.Text;

namespace GenAPK.Controllers
{
    public class GeneradorController : Controller
    {
        private readonly IDbMetadataRepository _repository;
        private readonly ApkBuilderService _apkService;

        public GeneradorController(IDbMetadataRepository repository, ApkBuilderService apkService)
        {
            _repository = repository;
            _apkService = apkService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Verificar si hay un perfil seleccionado
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

        [HttpGet]
        public async Task<IActionResult> GetDatabases()
        {
            try
            {
                // Verificar perfil seleccionado
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

        // ... resto del código sin cambios ...

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