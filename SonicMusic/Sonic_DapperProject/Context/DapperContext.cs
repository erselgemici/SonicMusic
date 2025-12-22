using Microsoft.Data.SqlClient;
using System.Data;

namespace Sonic_DapperProject.Context
{
    public class DapperContext
    {
        private readonly string _connectionString;

        public DapperContext(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }


        public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}
