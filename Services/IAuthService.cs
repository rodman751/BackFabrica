using CapaDapper.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    /// <summary>
    /// Defines the authentication business logic contract.
    /// Implementations validate credentials and issue JWT tokens.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Authenticates a user and returns a JWT token together with their profile information.
        /// </summary>
        /// <param name="usuario">Username to authenticate.</param>
        /// <param name="password">Plain-text password to validate.</param>
        /// <returns>A <see cref="LoginResponseDto"/> containing the signed JWT token and user data.</returns>
        Task<LoginResponseDto> LoginAsync(string usuario, string password);
    }
}
