using Dapper;
using System.Data;
using HNOne.Model.Models;
using Microsoft.Extensions.Logging;
using HNOne.Model;

namespace HNOne.DailyJob.Repositories
{
    public interface IDailyJobRepository
    {
        Task<IEnumerable<DailyJobConfigModel>> GetDailyJob();
        Task<ResponseModel> UpdateDailyJob(DailyJobConfigModel entity);
    }
    public class DailyJobRepository : IDailyJobRepository
    {
        private readonly ILogger<DailyJobRepository> _logger;
        private readonly IDapperDbContext _dapperDbContext;

        public DailyJobRepository(IDapperDbContext dbContext, ILogger<DailyJobRepository> logger)
        {
            _dapperDbContext = dbContext;
            _logger = logger;
        }

        #region Query
        /// <summary>
        /// Lấy danh sách các job
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<DailyJobConfigModel>> GetDailyJob()
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                string strQuery = "select * from DailyJobConfigs where IsCompleted = 0 and ExecuteDate = cast(getdate() as date)";
                var lstResult = await connection.QueryAsync<DailyJobConfigModel>(strQuery, commandTimeout: 500, commandType: CommandType.Text);
                return lstResult ?? new List<DailyJobConfigModel>();
            }    
        }
        #endregion

        /// <summary>
        /// cập nhật job
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateDailyJob(DailyJobConfigModel entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                if (string.IsNullOrEmpty(entity.sqlText)) return response;
                using (var connection = _dapperDbContext.CreateConnection())
                {
                    string strQuery = entity.sqlText;
                    var result = await connection.QueryFirstAsync<ResponseModel>(strQuery, commandTimeout: 500, commandType: CommandType.Text);
                    response.status = result?.status ?? 404;
                    response.message = result?.message ?? "404";
                    return response;
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"UpdateDailyJob: {entity.id}");
            }
            return response;
        }
    }
}
