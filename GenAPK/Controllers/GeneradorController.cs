using CapaDapper.DataService;
using GenAPK.Models;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace GenAPK.Controllers
{
    public class GeneradorController : Controller
    {
        private readonly IDbMetadataRepository _repository;
        private readonly ApkBuilderService _apkService; // El servicio que compila Flutter

        // Inyectamos ambos servicios
        public GeneradorController(IDbMetadataRepository repository, ApkBuilderService apkService)
        {
            _repository = repository;
            _apkService = apkService;
        }

        // GET: /Generador/Index
        // Carga la vista con el Dropdown de bases de datos
        public async Task<IActionResult> Index()
        {
            try
            {
                // 1. Usamos tu método existente para traer los nombres
                var dbs = await _repository.ObtenerNombresDeBasesDeDatosAsync();

                // Pasamos la lista a la vista mediante ViewBag
                ViewBag.Databases = dbs;

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar bases de datos: " + ex.Message;
                return View();
            }
        }

        // POST: /Generador/GenerarApk
        // Recibe el nombre de la DB seleccionada en el form
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

                // Guardar las rutas en TempData para permitir descargas posteriores
                TempData["ApkPath"] = buildResult.ApkPath;
                TempData["ZipPath"] = buildResult.SourceCodeZipPath;
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

        // Nueva acción para mostrar opciones de descarga
        public IActionResult Descargar()
        {
            if (TempData["ApkPath"] == null)
            {
                TempData["Error"] = "No hay archivos disponibles para descargar.";
                return RedirectToAction("Index");
            }

            ViewBag.ApkPath = TempData["ApkPath"];
            ViewBag.ZipPath = TempData["ZipPath"];
            ViewBag.DbName = TempData["DbName"];
            
            // Mantener en TempData para las descargas
            TempData.Keep();
            
            return View();
        }

        // Acción para descargar el APK
        [HttpGet]
        public async Task<IActionResult> DescargarApk()
        {
            string rutaApk = TempData["ApkPath"]?.ToString();
            string dbName = TempData["DbName"]?.ToString();
            
            if (string.IsNullOrEmpty(rutaApk) || !System.IO.File.Exists(rutaApk))
            {
                TempData["Error"] = "El archivo APK no está disponible.";
                return RedirectToAction("Index");
            }

            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(rutaApk);
            string nombreArchivo = $"App_{dbName}_{DateTime.Now:yyyyMMdd}.apk";

            return File(fileBytes, "application/vnd.android.package-archive", nombreArchivo);
        }

        // Acción para descargar el ZIP del código fuente
        [HttpGet]
        public async Task<IActionResult> DescargarZip()
        {
            string rutaZip = TempData["ZipPath"]?.ToString();
            string dbName = TempData["DbName"]?.ToString();
            
            if (string.IsNullOrEmpty(rutaZip) || !System.IO.File.Exists(rutaZip))
            {
                TempData["Error"] = "El archivo ZIP no está disponible.";
                return RedirectToAction("Index");
            }

            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(rutaZip);
            string nombreArchivo = Path.GetFileName(rutaZip);

            return File(fileBytes, "application/zip", nombreArchivo);
        }
    }
}
