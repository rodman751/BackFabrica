using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDapper.Dtos
{
    /// <summary>
    /// Carries the outcome of a credential validation executed by <c>sp_ValidarLoginFinal</c>.
    /// <see cref="EsExitoso"/> is derived from the stored procedure's output message.
    /// </summary>
    public class ValidarLoginResult
    {
        /// <summary>Authenticated user data returned by the stored procedure, or <c>null</c> on failure.</summary>
        public UsuarioLogin Usuario { get; set; }
        /// <summary>Role assigned to the user (e.g., <c>admin</c>, <c>user</c>).</summary>
        public string Rol { get; set; }
        /// <summary>Module origin that determines the client application's navigation context.</summary>
        public string ModuloOrigen { get; set; }
        /// <summary>Diagnostic message from the stored procedure. Starts with <c>"SUCCESS"</c> on success.</summary>
        public string Mensaje { get; set; }
        /// <summary>Returns <c>true</c> when <see cref="Mensaje"/> indicates a successful authentication.</summary>
        public bool EsExitoso => Mensaje?.StartsWith("SUCCESS") ?? false;
    }
}
