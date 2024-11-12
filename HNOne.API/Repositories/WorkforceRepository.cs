using HNOne.API.Repositories.Interfaces;
using HNOne.Common;
using HNOne.Model.Entities;
using HNOne.Model;
using Microsoft.EntityFrameworkCore;
using HNOne.Model.Models;
using Dapper;
using HNOne.API.Constants;
using System.Data;

namespace HNOne.API.Repositories
{
    /// <summary>
    /// công phép
    /// </summary>
    public class WorkforceRepository : IWorkforceRepository
    {
        private readonly MasterDbContext _dbContext;
        private readonly IDapperDbContext _dapperDbContext;
        private readonly IDateTimeHelper _dateTimeHelper;
        public WorkforceRepository(MasterDbContext dbContext
            , IDapperDbContext dapperDbContext, IDateTimeHelper dateTimeHelper)
        {
            _dbContext = dbContext;
            _dapperDbContext = dapperDbContext;
            _dateTimeHelper = dateTimeHelper;
        }

        #region Query
        public async Task<IEnumerable<LeaveConfigModel>> GetLeaveConfig(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                string query = "select T0.* from LeaveConfigs as T0 with(nolock)";
                var lstResult = await connection.QueryAsync<LeaveConfigModel>(query, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                return lstResult;
            }    
        }
        #endregion

        #region Command

        /// <summary>
        /// cập nhật thông tin cấu hình phép
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateLeaveConfig(string actionType, LeaveConfigs entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                if (actionType == ProcessConstants.POST_LEAVE_CONFIG)
                {
                    // Tạo mới
                    entity.Id = await _dbContext.LeaveConfigs.Select(m => m.Id).DefaultIfEmpty().MaxAsync() + 1;
                    entity.DateTracking = dateTimeNow;
                    entity.CreateDate = dateTimeNow;
                    await _dbContext.LeaveConfigs.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();
                    response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                    return response;
                }
                // cập nhật
                var data = await _dbContext.LeaveConfigs.FirstOrDefaultAsync(m => m.Id == entity.Id);
                if (data == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                data.Year = entity.Year;
                data.FromDate = entity.FromDate;
                data.ToDate = entity.ToDate;
                data.ExpiryDate = entity.ExpiryDate;
                data.AccrualDate = entity.AccrualDate;
                data.NumOfLeave = entity.NumOfLeave;
                data.NumOfYearIncrease = entity.NumOfYearIncrease;
                data.NumOfLeaveIncrease = entity.NumOfLeaveIncrease;
                data.NumOfLeaveTransfer = entity.NumOfLeaveTransfer;
                data.IsOffSaturday = entity.IsOffSaturday;
                data.IsOffSunday = entity.IsOffSunday;
                data.IsActive = entity.IsActive;
                data.DateTracking = dateTimeNow;
                data.UpdateDate = dateTimeNow;
                data.UserSign2 = entity.UserSign2;
                _dbContext.LeaveConfigs.Attach(data);
                _dbContext.Entry(data).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
            }
            catch (Exception ex)
            {
                response.status = StatusCodes.Status400BadRequest;
                response.message = ex.Message;
            }
            return response;
        }
        #endregion
    }
}
