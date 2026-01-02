using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDapper.Cadena
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionTemplate;
        private readonly IDatabaseContext _dbContext;

        public DbConnectionFactory(IConfiguration configuration, IDatabaseContext dbContext)
        {
            _connectionTemplate = configuration.GetConnectionString("TemplateConnection");
            _dbContext = dbContext;
        }

        public IDbConnection CreateConnection()
        {
            if (string.IsNullOrEmpty(_dbContext.CurrentDb))
                throw new Exception("La base de datos no ha sido establecida para esta petición.");

            var connectionString = string.Format(_connectionTemplate, _dbContext.CurrentDb);
            return new SqlConnection(connectionString);
        }
    }
}
