using CapaDapper.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class AuthRepository : IAuthRepository
    {
        public async Task<ResponseLoginDto> ValidarUsuario(string usuario, string password)
        {
            // Aquí iría la lógica para validar el usuario contra la base de datos usando Dapper
            // Por ahora, retornamos un objeto simulado si el usuario y contraseña son correctos
            if (usuario == "admin" && password == "admin")
            {
                return new ResponseLoginDto  { Id = 1, UserName = "admin" }; // Simulamos un usuario válido
            }
            return null; // Usuario no válido
        }
    }
}
