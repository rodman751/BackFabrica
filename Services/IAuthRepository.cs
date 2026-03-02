using CapaDapper.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    /// <summary>
    /// Defines the data-access contract for user credential validation.
    /// </summary>
    public interface IAuthRepository
    {
        /// <summary>
        /// Validates the supplied username and password against stored credentials.
        /// </summary>
        /// <param name="username">Username to look up.</param>
        /// <param name="passwordHash">Plain-text password sent by the client.</param>
        /// <returns>A <see cref="ValidarLoginResult"/> indicating success or failure.</returns>
        Task<ValidarLoginResult> ValidarUserPassAsync(string username, string passwordHash);
    }
}
