using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IAuthService
    {
        // Devuelve el string del token si el login es correcto
        Task<string> LoginAsync(string usuario, string password);
    }
}
