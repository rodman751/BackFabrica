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
    /// <summary>
    /// Provides data-access operations for user authentication.
    /// Executes the <c>sp_ValidarLoginFinal</c> stored procedure against the
    /// <c>master</c> database, which is the central user registry for all modules.
    /// </summary>
    public class AuthRepository : IAuthRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public AuthRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        #region Validar Login
        /// <summary>
        /// Validates the supplied username and password hash against the stored credentials
        /// by invoking <c>sp_ValidarLoginFinal</c>.
        /// Returns a failed result object instead of throwing on authentication errors,
        /// so the caller can distinguish between database errors and invalid credentials.
        /// </summary>
        /// <param name="username">Username to look up.</param>
        /// <param name="passwordHash">Plain-text password sent by the client (hashed inside the stored procedure).</param>
        /// <returns>
        /// A <see cref="ValidarLoginResult"/> containing the matched user, role, module origin,
        /// and a diagnostic message. <c>EsExitoso</c> is <c>false</c> on failure.
        /// </returns>
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

                    var usuario = await connection.QueryFirstOrDefaultAsync<UsuarioLogin>(
                        "sp_ValidarLoginFinal",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

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
                return new ValidarLoginResult
                {
                    Usuario = null,
                    Rol = null,
                    ModuloOrigen = null,
                    Mensaje = $"Error al validar credenciales: {ex.Message}",
                };
            }
        }

        #endregion
    }

}
