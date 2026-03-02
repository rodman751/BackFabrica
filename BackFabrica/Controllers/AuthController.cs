using CapaDapper.Cadena;
using CapaDapper.DataService;
using CapaDapper.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.Xml.Linq;

namespace BackFabrica.Controllers
{
    /// <summary>
    /// Handles user authentication and database module creation.
    /// Credentials are supplied via HTTP headers rather than a request body
    /// to simplify integration with the Flutter mobile client.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IDatabaseContext _dbContext;
        private readonly IDbMetadataRepository _dbService;

        public AuthController(IAuthService authService, IDatabaseContext databaseContext, IDbMetadataRepository dbMetadataRepository)
        {
            _authService = authService;
            _dbContext = databaseContext;
            _dbService = dbMetadataRepository;
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// Expects <c>X-DbName</c>, <c>X-Usuario</c>, and <c>X-Password</c> headers.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromHeader(Name = "X-DbName")] string dbName)
        {
            string usuario = Request.Headers["X-Usuario"];
            string password = Request.Headers["X-Password"];

            _dbContext.CurrentDb = dbName;
            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(password))
            {
                return BadRequest(new { message = "Faltan las cabeceras 'X-Usuario' o 'X-Password'" });
            }
            try
            {
                var result = await _authService.LoginAsync(usuario, password);
                return Ok(new
                {
                    token = result.Token,
                    id = result.Id,
                    username = result.Username,
                    email = result.Email,
                    role = result.Role,
                    moduloOrigen = result.ModuloOrigen
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Creates a new database module (schema + stored procedures) from the provided definition.
        /// </summary>
        [HttpPost("crear-modulo")]
        public async Task<IActionResult> CrearModulo([FromBody] RequestCrearModuloDto request)
        {
            var resultado = await _dbService.CrearNuevoModuloAsync(request);

            if (resultado)
            {
                return Ok(new { mensaje = $"Base de datos {request.NombreDb} creada con éxito." });
            }

            return StatusCode(500, "Error interno al procesar la creación de la base de datos.");
        }
    }
}
