using Microsoft.Data.SqlClient;
using System.Data;

namespace HNOne.API.Repositories
{
    public interface IDapperDbContext
    {
        IDbConnection CreateConnection();
    }
    public class DapperDbContext : IDapperDbContext
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;
        public DapperDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DbConnection") + "";
        }

        public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}
