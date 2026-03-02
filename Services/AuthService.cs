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
    /// <summary>
    /// Implements the authentication business logic layer.
    /// Delegates credential validation to <see cref="IAuthRepository"/> and, on success,
    /// issues a signed JWT token configured from application settings.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;
        private readonly IAuthRepository _repository;

        public AuthService(IConfiguration config, IAuthRepository repository)
        {
            _config = config;
            _repository = repository;
        }

        /// <summary>
        /// Validates the supplied credentials and returns a JWT token along with user profile data.
        /// Throws <see cref="UnauthorizedAccessException"/> when the credentials are invalid.
        /// </summary>
        /// <param name="usuario">Username to authenticate.</param>
        /// <param name="password">Plain-text password to validate against the stored hash.</param>
        /// <returns>A <see cref="LoginResponseDto"/> containing the JWT token and user information.</returns>
        public async Task<LoginResponseDto> LoginAsync(string usuario, string password)
        {
            var user = await _repository.ValidarUserPassAsync(usuario, password);

            if (user.EsExitoso == false)
            {
                throw new UnauthorizedAccessException(user.Mensaje);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Usuario.Username),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(8),
                signingCredentials: creds
            );

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
