using CapaDapper.Cadena;
using CapaDapper.Dtos;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class AuthRepository : IAuthRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public AuthRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }
       

        #region Validar Login
        public async Task<ValidarLoginResult> ValidarUserPassAsync( string username, string passwordHash)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@p_username", username, DbType.String, ParameterDirection.Input);
                parameters.Add("@p_password_hash", passwordHash, DbType.String, ParameterDirection.Input);
                parameters.Add("@p_rol_output", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);
                parameters.Add("@p_modulo_output", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);
                parameters.Add("@p_msj", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);

                // Ejecutamos el SP y obtenemos el resultado (SELECT final)
                var usuario = await connection.QueryFirstOrDefaultAsync<UsuarioLogin>(
                    "sp_ValidarLoginFinal",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                // Obtenemos los parámetros de salida
                var result = new ValidarLoginResult
                {
                    Usuario = usuario,
                    Rol = parameters.Get<string>("@p_rol_output"),
                    ModuloOrigen = parameters.Get<string>("@p_modulo_output"),
                    Mensaje = parameters.Get<string>("@p_msj")
                };

                return result;
            }
        }

        #endregion
    }

}

