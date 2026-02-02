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
                // 1. Obtener el Esquema JSON usando tu lógica actual
                // (Dapper se conecta y trae el JSON de esa DB)
                var jsonSchema = await _repository.ObtenerEsquemaJsonAsync(selectedDb);

                if (string.IsNullOrEmpty(jsonSchema))
                {
                    TempData["Error"] = "El esquema obtenido está vacío o la DB no es accesible.";
                    return RedirectToAction("Index");
                }

                // 2. Llamar al servicio que modifica Flutter y Compila (definido en el paso anterior)
                // Esto puede tardar entre 30seg y 2min dependiendo de tu PC
                var buildResult = await _apkService.GenerarApkAsync(selectedDb, jsonSchema);

                if (buildResult == null || string.IsNullOrEmpty(buildResult.ApkPath))
                {
                    TempData["Error"] = "No se pudo generar el APK o la ruta está vacía.";
                    return RedirectToAction("Index");
                }

                string rutaApkGenerado = buildResult.ApkPath;

                // 3. Leer el archivo y forzar la descarga en el navegador
                byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(rutaApkGenerado);
                string nombreArchivo = $"App_{selectedDb}_{DateTime.Now:yyyyMMdd}.apk";

                return File(fileBytes, "application/vnd.android.package-archive", nombreArchivo);
            }
            catch (Exception ex)
            {
                // Si algo falla (ej: error de compilación de Flutter) volvemos a la vista con el error
                TempData["Error"] = $"Error en la generación: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}
