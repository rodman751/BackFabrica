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
            string selectedProfile = "Local";

            // LEER SESIÓN SIN MÉTODOS DE EXTENSIÓN
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null && session.TryGetValue("SelectedProfile", out byte[] value))
            {
                selectedProfile = Encoding.UTF8.GetString(value);
            }

            var connectionTemplate = _configuration[$"ConnectionProfiles:{selectedProfile}:ConnectionString"]
                                   ?? _configuration.GetConnectionString("TemplateConnection");

            if (string.IsNullOrEmpty(_dbContext.CurrentDb))
                throw new Exception("La base de datos no ha sido establecida.");

            var connectionString = string.Format(connectionTemplate, _dbContext.CurrentDb);
            return new SqlConnection(connectionString);
        }
    }
}