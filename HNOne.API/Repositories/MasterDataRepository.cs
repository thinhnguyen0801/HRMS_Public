using Dapper;
using HNOne.API.Constants;
using HNOne.API.Repositories.Interfaces;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;

namespace HNOne.API.Repositories
{
    public class MasterDataRepository : IMasterDataRepository
    {
        private readonly MasterDbContext _dbContext;
        private readonly IDapperDbContext _dapperDbContext;
        private readonly IDateTimeHelper _dateTimeHelper;
        public MasterDataRepository(MasterDbContext dbContext
            , IDapperDbContext dapperDbContext, IDateTimeHelper dateTimeHelper) 
        {
            _dbContext = dbContext;
            _dapperDbContext = dapperDbContext;
            _dateTimeHelper = dateTimeHelper;
        }

        #region Query

        public async Task<IEnumerable<Branchs>> GetBranch()
        {
            var lstBranch = await _dbContext.Branchs.Where(m=> m.IsDelete == false).ToListAsync();
            return lstBranch;
        }

        public async Task<IEnumerable<Menus>> GetMenu()
        {
            var lstMenus = await _dbContext.Menus.Where(m => m.IsVisible).ToListAsync();
            return lstMenus;
        }

        #endregion 

        #region Command

        /// <summary>
        /// Thêm chi nhánh
        /// </summary>
        /// <param name="process"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> AddBranch(Branchs entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var connection = _dapperDbContext.CreateConnection())
                {
                    string commandText = @$"select {StoreConstants.FUNC_GET_VOUCHER}(@Type)";
                    string? voucherNo = await connection.QueryFirstOrDefaultAsync<string>(commandText, param: new { Type = GlobalConstants.TABLE_BRANCH }, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                    if (string.IsNullOrEmpty(voucherNo))
                    {
                        response.status = StatusCodes.Status204NoContent;
                        response.message = MessageConstants.MESSAGE_VOUCHER_NO_MISSING;
                        return response;
                    }
                    entity.BranchId = await _dbContext.Branchs.Select(m=>m.BranchId).DefaultIfEmpty().MaxAsync() + 1;
                    entity.BranchCode = voucherNo;
                    entity.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                    entity.CreateDate = _dateTimeHelper.GetCurrentVietnamTime();
                    await _dbContext.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();
                    response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                }
                return response;
            }
            catch (Exception) { throw; }
        }


        /// <summary>
        /// Thêm chi nhánh
        /// </summary>
        /// <param name="process"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateBranch(Branchs entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                var branch = await _dbContext.Branchs.FirstOrDefaultAsync(m => m.BranchId == entity.BranchId);
                if(branch == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                branch.BranchName = entity.BranchName;
                branch.Address = entity.Address;
                branch.ImgUrl = entity.ImgUrl;
                branch.PhoneNumber = entity.PhoneNumber;
                branch.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                branch.UpdateDate = _dateTimeHelper.GetCurrentVietnamTime();
                branch.UserSign2 = entity.UserSign2;
                _dbContext.Branchs.Attach(branch);
                _dbContext.Entry(branch).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
                return response;
            }
            catch (Exception){ throw; }
        }


        #endregion
    }
}
