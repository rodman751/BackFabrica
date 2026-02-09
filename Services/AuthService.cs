using CapaDapper.Cadena;
using CapaDapper.Dtos;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Services
{

    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;
        private readonly IAuthRepository _repository;


        public AuthService(IConfiguration config, IAuthRepository repository)
        {
            _config = config;
            _repository = repository;
 
        }

        public async Task<LoginResponseDto> LoginAsync(string usuario, string password)
        {
            // 1. Validar contra la Base de Datos (usando tu Repo con Dapper)
            var user = await _repository.ValidarUserPassAsync(usuario, password);

            // Si el usuario no existe o las credenciales son incorrectas
            if (user.EsExitoso == false)
            {
                throw new UnauthorizedAccessException(user.Mensaje);
            }

            // 2. Crear los "Claims" (La información que va DENTRO del token)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Usuario.Id.ToString()), // ID del usuario
                new Claim(ClaimTypes.Name, user.Usuario.Username),                 // Nombre de usuario

            };

            // 3. Crear la llave de seguridad (debe coincidir con appsettings)
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 4. Configurar el Token
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(8),
                signingCredentials: creds
            );

            // 5. Retornar token + datos del usuario
            return new LoginResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Id = user.Usuario?.Id,
                Username = user.Usuario?.Username,
                Email = user.Usuario?.Email,
                Role = user.Rol,
                ModuloOrigen = user.ModuloOrigen
            };
        }
    }
}