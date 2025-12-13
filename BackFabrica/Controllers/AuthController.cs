using Dapper.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace BackFabrica.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login()
        {
            // 1. Intentamos leer los headers personalizados
            // Usamos "X-" porque es la convención para headers propios, 
            // pero puedes llamarlos simplemente "Usuario" y "Password" si prefieres.
            string usuario = Request.Headers["X-Usuario"];
            string password = Request.Headers["X-Password"];

            // 2. Validamos que vengan los datos
            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(password))
            {
                return BadRequest(new { message = "Faltan las cabeceras 'X-Usuario' o 'X-Password'" });
            }

            // 3. Llamamos al servicio (esto es igual que antes)
            var token = await _authService.LoginAsync(usuario, password);

            if (token == null)
            {
                return Unauthorized(new { message = "Credenciales incorrectas" });
            }

            return Ok(new { token = token });
        }
        
    }
}
