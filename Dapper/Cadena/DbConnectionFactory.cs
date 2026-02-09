using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Text; // Necesario para Encoding

namespace CapaDapper.Cadena
{
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

        public IDbConnection CreateConnection()
        {
            string selectedProfile = "Local"; // Valor por defecto inicial

            try
            {
                var httpContext = _httpContextAccessor.HttpContext;

                // 1. PRIORIDAD ALTA: Intentar leer el perfil desde el header (para API/APK)
                if (httpContext?.Request?.Headers != null && httpContext.Request.Headers.ContainsKey("X-Connection-Profile"))
                {
                    var headerValue = httpContext.Request.Headers["X-Connection-Profile"].ToString();
                    if (!string.IsNullOrEmpty(headerValue))
                    {
                        selectedProfile = headerValue;
                    }
                }
                // 2. PRIORIDAD MEDIA: Si no hay header, intentar leer de la sesión (para MVC)
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
                // Si hay error al leer header o sesión, usar el perfil por defecto
                Console.WriteLine($"Advertencia al leer perfil de conexión: {ex.Message}. Usando perfil por defecto: {selectedProfile}");
            }

            // 3. Validar Configuración
            var connectionTemplate = _configuration[$"ConnectionProfiles:{selectedProfile}:ConnectionString"]
                                    ?? _configuration.GetConnectionString("TemplateConnection");

            if (string.IsNullOrEmpty(connectionTemplate))
                throw new InvalidOperationException($"No se encontró el perfil de conexión: {selectedProfile}");

            // 4. Validar DB seleccionada
            if (string.IsNullOrEmpty(_dbContext.CurrentDb))
                throw new Exception("Debes seleccionar una base de datos primero.");

            var connectionString = string.Format(connectionTemplate, _dbContext.CurrentDb);

            return new SqlConnection(connectionString);
        }
    }
}