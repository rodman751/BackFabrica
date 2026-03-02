using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Text;

namespace CapaDapper.Cadena
{
    /// <summary>
    /// Creates SQL database connections by resolving the active connection profile from the HTTP request context.
    /// Profile priority: <c>X-Connection-Profile</c> header → session value → default <c>Local</c>.
    /// The resolved profile's template connection string is formatted with <see cref="IDatabaseContext.CurrentDb"/>.
    /// </summary>
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly IDatabaseContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DbConnectionFactory(IConfiguration configuration, IDatabaseContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc/>
        public IDbConnection CreateConnection()
        {
            string selectedProfile = "Local";

            try
            {
                var httpContext = _httpContextAccessor.HttpContext;

                if (httpContext?.Request?.Headers != null && httpContext.Request.Headers.ContainsKey("X-Connection-Profile"))
                {
                    var headerValue = httpContext.Request.Headers["X-Connection-Profile"].ToString();
                    if (!string.IsNullOrEmpty(headerValue))
                    {
                        //selectedProfile = headerValue;
                        selectedProfile = "Local";
                    }
                }
                else
                {
                    var session = httpContext?.Session;
                    if (session != null && session.TryGetValue("SelectedProfile", out byte[] value))
                    {
                        var sessionValue = Encoding.UTF8.GetString(value);
                        if (!string.IsNullOrEmpty(sessionValue))
                        {
                            selectedProfile = sessionValue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex;
            }

            var connectionTemplate = _configuration[$"ConnectionProfiles:{selectedProfile}:ConnectionString"]
                                    ?? _configuration.GetConnectionString("TemplateConnection");

            if (string.IsNullOrEmpty(connectionTemplate))
                throw new InvalidOperationException($"No se encontró el perfil de conexión: {selectedProfile}");

            if (string.IsNullOrEmpty(_dbContext.CurrentDb))
                throw new Exception("Debes seleccionar una base de datos primero.");

            var connectionString = string.Format(connectionTemplate, _dbContext.CurrentDb);

            return new SqlConnection(connectionString);
        }
    }
}