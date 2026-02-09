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
        public async Task<ValidarLoginResult> ValidarUserPassAsync(string username, string passwordHash)
        {
            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();
                    connection.ChangeDatabase("master");
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
            catch (Exception ex)
            {
                // Log detallado del error
                Console.WriteLine($"Error en ValidarUserPassAsync: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                // Retornar un resultado de error en lugar de propagar la excepción
                return new ValidarLoginResult
                {
                    Usuario = null,
                    Rol = null,
                    ModuloOrigen = null,
                    Mensaje = $"Error al validar credenciales: {ex.Message}",
                    //EsExitoso = false
                };
            }
        }

        #endregion
    }

}