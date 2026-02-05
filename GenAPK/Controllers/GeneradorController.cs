using CapaDapper.DataService;
using GenAPK.Models;
using Microsoft.AspNetCore.Mvc;
using Services;

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

                // Guardar en TempData con persistencia mejorada
                TempData["ApkPath"] = buildResult.ApkPath;
                TempData["ZipPath"] = buildResult.SourceCodeZipPath;
                TempData["DbName"] = selectedDb;
                TempData["Success"] = "APK y código fuente generados correctamente.";

                // IMPORTANTE: Redirigir sin devolver archivo
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
            // Peek en lugar de leer directamente para no consumir
            if (TempData.Peek("ApkPath") == null)
            {
                TempData["Error"] = "No hay archivos disponibles para descargar.";
                return RedirectToAction("Index");
            }

            // Usar Peek para no consumir los valores
            ViewBag.ApkPath = TempData.Peek("ApkPath");
            ViewBag.ZipPath = TempData.Peek("ZipPath");
            ViewBag.DbName = TempData.Peek("DbName");
            ViewBag.Success = TempData.Peek("Success");

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> DescargarApk()
        {
            // Usar Peek para no consumir el valor
            string rutaApk = TempData.Peek("ApkPath")?.ToString();
            string dbName = TempData.Peek("DbName")?.ToString();

            if (string.IsNullOrEmpty(rutaApk) || !System.IO.File.Exists(rutaApk))
            {
                TempData["Error"] = "El archivo APK no está disponible.";
                return RedirectToAction("Index");
            }

            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(rutaApk);
            string nombreArchivo = $"App_{dbName}_{DateTime.Now:yyyyMMdd}.apk";

            // Mantener TempData para permitir múltiples descargas
            TempData.Keep();

            return File(fileBytes, "application/vnd.android.package-archive", nombreArchivo);
        }

        [HttpGet]
        public async Task<IActionResult> DescargarZip()
        {
            // Usar Peek para no consumir el valor
            string rutaZip = TempData.Peek("ZipPath")?.ToString();
            string dbName = TempData.Peek("DbName")?.ToString();

            if (string.IsNullOrEmpty(rutaZip) || !System.IO.File.Exists(rutaZip))
            {
                TempData["Error"] = "El archivo ZIP no está disponible.";
                return RedirectToAction("Index");
            }

            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(rutaZip);
            string nombreArchivo = Path.GetFileName(rutaZip);

            // Mantener TempData para permitir múltiples descargas
            TempData.Keep();

            return File(fileBytes, "application/zip", nombreArchivo);
        }
    }
}