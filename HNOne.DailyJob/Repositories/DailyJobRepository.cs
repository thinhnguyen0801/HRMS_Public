using Dapper;
using System.Data;
using HNOne.Model.Models;
using Microsoft.Extensions.Logging;
using HNOne.Model;
using Azure;
using static Dapper.SqlMapper;
using HNOne.Model.Entities;
using Azure.Core;

namespace HNOne.DailyJob.Repositories
{
    public interface IDailyJobRepository
    {
        Task<IEnumerable<DailyJobConfigModel>> GetDailyJob();
        Task<ResponseModel> CalculateALInfo();
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
                response.status = 400;
                response.message = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Tính toán lại thông tin phép năm
        /// </summary>
        /// <returns></returns>
        public async Task<ResponseModel> CalculateALInfo()
        {
            ResponseModel response = new ResponseModel();
            try
            {

                using (var connection = _dapperDbContext.CreateConnection())
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@Year", DateTime.Now.Year, DbType.Int32);
                    // lấy ra cấu hình tính phép của nhân viên của các chi nhánh
                    string strQuery = $"select * from LeaveConfigs as T0 with(nolock) where T0.IsDelete = 0 and T0.IsActive = '1' and T0.[Year] = @Year";
                    var result = await connection.QueryAsync<LeaveConfigs>(strQuery, param: parameters, commandTimeout: 500, commandType: CommandType.Text);
                    if(result != null && result.Any())
                    {
                        foreach(var item in result)
                        {
                            if (item.AccrualDate != DateTime.Now.Date.Day) continue;
                            parameters = new DynamicParameters();
                            parameters.Add("@UserId", -1, DbType.Int32);
                            parameters.Add("@BranchId", item.BranchId, DbType.Int32);
                            parameters.Add("@Year", item.Year, DbType.Int32);
                            parameters.Add("@EmployeeIds", "", DbType.String); // tính cho tất cả nhân viên
                            await connection.ExecuteAsync("H1_ANNUAL_LEAVE_INFO_UPDATE", param: parameters, commandTimeout: 500, commandType: CommandType.StoredProcedure);
                        }    
                    }
                    return response;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"CalculateALInfo");
                response.status = 400;
                response.message = ex.Message;
            }
            return response;
        }
    }
}
