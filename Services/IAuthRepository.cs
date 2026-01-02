using CapaDapper.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IAuthRepository
    {
  
        Task<ValidarLoginResult> ValidarUserPassAsync(string username, string passwordHash);
    }
}
