using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDapper.Dtos
{
    /// <summary>
    /// Represents the authenticated user's profile data returned by <c>sp_ValidarLoginFinal</c>.
    /// Embedded inside <see cref="ValidarLoginResult"/> and serialized into the JWT response.
    /// </summary>
    public class UsuarioLogin
    {
        /// <summary>Unique identifier of the user record.</summary>
        public int Id { get; set; }
        /// <summary>Login username used for authentication.</summary>
        public string Username { get; set; }
        /// <summary>User's email address.</summary>
        public string Email { get; set; }
        /// <summary>Role assigned to the user (e.g., <c>admin</c>, <c>user</c>).</summary>
        public string Rol { get; set; }
        /// <summary>Module origin that determines the client application's navigation context.</summary>
        public string ModuloOrigen { get; set; }
    }
}
