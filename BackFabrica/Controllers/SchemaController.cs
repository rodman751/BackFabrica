using CapaDapper.DataService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackFabrica.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchemaController : ControllerBase
    {
        private readonly IDbMetadataRepository _repository;

        public SchemaController(IDbMetadataRepository repository)
        {
            _repository = repository;
        }

        // GET: api/schema/databases
        // Este lo llamas primero para llenar el "Select/Dropdown" en la App Móvil
        [HttpGet("databases")]
        public async Task<IActionResult> GetDatabases()
        {
            var dbs = await _repository.ObtenerNombresDeBasesDeDatosAsync();
            return Ok(dbs);
        }

        // GET: api/schema/generate?db=VentasDB
        // Este lo llamas cuando el usuario selecciona una y le da a "Generar App"
        [HttpGet("generate")]
        public async Task<IActionResult> GetSchema([FromQuery] string db)
        {
            if (string.IsNullOrEmpty(db))
                return BadRequest("Debes seleccionar una base de datos.");

            try
            {
                // Dapper se conecta a esa DB y trae el JSON
                var jsonSchema = await _repository.ObtenerEsquemaJsonAsync(db);

                // Retornamos el JSON tal cual (Content devuelve string como application/json)
                return Content(jsonSchema, "application/json");
            }
            catch (Exception ex)
            {
                // Manejo de errores (por si la DB no existe o no hay acceso)
                return BadRequest($"Error al conectar con la base de datos '{db}': {ex.Message}");
            }
        }
    }
}
