using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVEA.Repository.Infrastructure
{
    public class SqlConnectionFactory: ISqlConnectionFactory
    {
        private SqlConnection? _connection = null;

        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        
        public SqlConnectionFactory(IConfiguration configuration, ILogger<SqlConnectionFactory> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Dispose();
                _connection = null;
            }
        }
        public IDbConnection GetConnection()
        {
            if (_connection == null)
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                _connection = new SqlConnection(connectionString);
                _connection.Open();
            }
            else if (_connection.State != System.Data.ConnectionState.Open)
            {
                _connection.Open();
            }
            return _connection;
        }

       
    }
}
