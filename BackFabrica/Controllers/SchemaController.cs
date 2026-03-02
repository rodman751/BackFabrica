using CapaDapper.DataService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackFabrica.Controllers
{
    /// <summary>
    /// Exposes database metadata endpoints used by the Flutter client to
    /// discover available databases and retrieve their schema definitions.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SchemaController : ControllerBase
    {
        private readonly IDbMetadataRepository _repository;

        public SchemaController(IDbMetadataRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Returns the names of all user databases available on the server.
        /// The client uses this list to populate the database selection dropdown.
        /// </summary>
        // GET: api/schema/databases
        [HttpGet("databases")]
        public async Task<IActionResult> GetDatabases()
        {
            var dbs = await _repository.ObtenerNombresDeBasesDeDatosAsync();
            return Ok(dbs);
        }

        /// <summary>
        /// Returns the full JSON schema for the specified database,
        /// including tables, columns, primary keys, and foreign keys.
        /// </summary>
        /// <param name="db">Name of the target database.</param>
        // GET: api/schema/generate?db=VentasDB
        [HttpGet("generate")]
        public async Task<IActionResult> GetSchema([FromQuery] string db)
        {
            if (string.IsNullOrEmpty(db))
                return BadRequest("Debes seleccionar una base de datos.");

            try
            {
                var jsonSchema = await _repository.ObtenerEsquemaJsonAsync(db);
                return Content(jsonSchema, "application/json");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al conectar con la base de datos '{db}': {ex.Message}");
            }
        }
    }
}
