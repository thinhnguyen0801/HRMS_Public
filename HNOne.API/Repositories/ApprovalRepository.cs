using Dapper;
using HNOne.API.Constants;
using HNOne.Common;
using HNOne.Model.Models;
using HNOne.Model;
using System.Data;
using HNOne.Model.Entities;
using Microsoft.EntityFrameworkCore;
using HNOne.API.Repositories.Interfaces;
using System.Diagnostics.Contracts;

namespace HNOne.API.Repositories
{
    public class ApprovalRepository : IApprovalRepository
    {
        private readonly MasterDbContext _dbContext;
        private readonly IDapperDbContext _dapperDbContext;
        private readonly IDateTimeHelper _dateTimeHelper;

        public ApprovalRepository(MasterDbContext dbContext
            , IDapperDbContext dapperDbContext, IDateTimeHelper dateTimeHelper)
        {
            _dbContext = dbContext;
            _dapperDbContext = dapperDbContext;
            _dateTimeHelper = dateTimeHelper;
        }

        #region Query
        
        #endregion

        #region Command

        /// <summary>
        /// gửi phê duyệt
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> AddApproval(Approvals entity)
        {
            bool isTran = false;
            bool isRollback = false;
            ResponseModel response = new ResponseModel();
            try
            {
                DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                if(entity.DocEntry < 1)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                await _dbContext.Database.BeginTransactionAsync();
                // Tạo mới
                isTran = true;
                entity.DateTracking = dateTimeNow;
                entity.CreateDate = dateTimeNow;
                await _dbContext.Approvals.AddAsync(entity);
                // cập nhật tình trạng chứng từ
                switch(entity.ObjType)
                {
                    case GlobalConstants.TABLE_CONTRACT:
                        var contract = await _dbContext.Contracts.FirstOrDefaultAsync(m => m.Id == entity.DocEntry);
                        if(contract == null)
                        {
                            response.status = StatusCodes.Status404NotFound;
                            response.message = $"ObjType {entity.ObjType} was not provider!!!";
                            await _dbContext.Database.RollbackTransactionAsync();
                            return response;
                        }
                        contract.StatusCode = CommonConstants.STATUS_CODE_APPROVAL_PENDING; // ĐÃ GỬI YÊU CẦU PHÊ DUYỆT
                        contract.DateTracking = dateTimeNow;
                        _dbContext.Contracts.Attach(contract);
                        _dbContext.Entry(contract).State = EntityState.Modified;
                        break;
                    default:
                        response.status = StatusCodes.Status404NotFound;
                        response.message = $"ObjType {entity.ObjType} was not provider!!!";
                        await _dbContext.Database.RollbackTransactionAsync();
                        return response;
                }
                await _dbContext.SaveChangesAsync();
                await _dbContext.Database.CommitTransactionAsync();
                response.message = MessageConstants.MESSAGE_SEND_APPROVAL_SUCCESS;
                return response;
            }
            catch (Exception ex)
            {
                if(isTran) await _dbContext.Database.RollbackTransactionAsync();
                response.status = StatusCodes.Status400BadRequest;
                response.message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseModel> UpdateApproval(string actionType, Approvals entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                // cập nhật
                var data = await _dbContext.Approvals.FirstOrDefaultAsync(m => m.Id == entity.Id);
                if (data == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                data.StatusCode = entity.StatusCode;
                data.ApprovalRemark = entity.ApprovalRemark;
                data.DateTracking = dateTimeNow;
                data.UpdateDate = dateTimeNow;
                data.UserSign2 = entity.UserSign2;
                _dbContext.Approvals.Attach(data);
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
