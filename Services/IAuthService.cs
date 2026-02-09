using CapaDapper.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IAuthService
    {
        // Devuelve los datos completos del login (token + usuario + rol + moduloOrigen)
        Task<LoginResponseDto> LoginAsync(string usuario, string password);
    }
}
