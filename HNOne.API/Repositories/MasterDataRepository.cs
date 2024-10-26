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

        public async Task<IEnumerable<Departments>> GetDepartment()
        {
            var lstMenus = await _dbContext.Departments.Where(m => m.IsActive).ToListAsync();
            return lstMenus;
        }
        public async Task<IEnumerable<Titles>> GetTitle()
        {
            var lstMenus = await _dbContext.Titles.Where(m => m.IsActive).ToListAsync();
            return lstMenus;
        }
        public async Task<IEnumerable<Positions>> GetPosition()
        {
            var lstMenus = await _dbContext.Positions.Where(m => m.IsActive).ToListAsync();
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
                    await _dbContext.Branchs.AddAsync(entity);
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

        /// <summary>
        /// Thêm phòng ban
        /// </summary>
        /// <param name="process"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> AddDepartment(Departments entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var connection = _dapperDbContext.CreateConnection())
                {
                    string commandText = @$"select {StoreConstants.FUNC_GET_VOUCHER}(@Type)";
                    string? voucherNo = await connection.QueryFirstOrDefaultAsync<string>(commandText, param: new { Type = GlobalConstants.TABLE_DEPARTMENT }, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                    if (string.IsNullOrEmpty(voucherNo))
                    {
                        response.status = StatusCodes.Status204NoContent;
                        response.message = MessageConstants.MESSAGE_VOUCHER_NO_MISSING;
                        return response;
                    }
                    entity.Code = voucherNo;
                    entity.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                    entity.CreateDate = _dateTimeHelper.GetCurrentVietnamTime();
                    await _dbContext.Departments.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();
                    response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                }
                return response;
            }
            catch (Exception) { throw; }
        }


        /// <summary>
        /// Thêm cập nhật phòng ban
        /// </summary>
        /// <param name="process"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateDepartment(Departments entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                var department = await _dbContext.Departments.FirstOrDefaultAsync(m => m.Id == entity.Id);
                if (department == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                department.Name = entity.Name;
                department.ManagerId = entity.ManagerId;
                department.HeadId = entity.HeadId;
                department.AssistantManagerIds = entity.AssistantManagerIds;
                department.Remark = entity.Remark;
                department.IsActive = entity.IsActive;
                department.BranchId = entity.BranchId;
                department.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                department.UpdateDate = _dateTimeHelper.GetCurrentVietnamTime();
                department.UserSign2 = entity.UserSign2;
                _dbContext.Departments.Attach(department);
                _dbContext.Entry(department).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
                return response;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// Thêm chức vụ
        /// </summary>
        /// <param name="process"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> AddPosition(Positions entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var connection = _dapperDbContext.CreateConnection())
                {
                    string commandText = @$"select {StoreConstants.FUNC_GET_VOUCHER}(@Type)";
                    string? voucherNo = await connection.QueryFirstOrDefaultAsync<string>(commandText, param: new { Type = GlobalConstants.TABLE_POSITION }, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                    if (string.IsNullOrEmpty(voucherNo))
                    {
                        response.status = StatusCodes.Status204NoContent;
                        response.message = MessageConstants.MESSAGE_VOUCHER_NO_MISSING;
                        return response;
                    }
                    entity.Code = voucherNo;
                    entity.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                    entity.CreateDate = _dateTimeHelper.GetCurrentVietnamTime();
                    await _dbContext.Positions.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();
                    response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                }
                return response;
            }
            catch (Exception) { throw; }
        }


        /// <summary>
        /// Cập nhật chức vụ
        /// </summary>
        /// <param name="process"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdatePosition(Positions entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                var data = await _dbContext.Positions.FirstOrDefaultAsync(m => m.Id == entity.Id);
                if (data == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                data.Name = entity.Name;
                data.LevelCode = entity.LevelCode;
                data.Remark = entity.Remark;
                data.IsActive = entity.IsActive;
                data.BranchId = entity.BranchId;
                data.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                data.UpdateDate = _dateTimeHelper.GetCurrentVietnamTime();
                data.UserSign2 = entity.UserSign2;
                _dbContext.Positions.Attach(data);
                _dbContext.Entry(data).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
                return response;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// Thêm phòng ban
        /// </summary>
        /// <param name="process"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> AddTitle(Titles entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var connection = _dapperDbContext.CreateConnection())
                {
                    string commandText = @$"select {StoreConstants.FUNC_GET_VOUCHER}(@Type)";
                    string? voucherNo = await connection.QueryFirstOrDefaultAsync<string>(commandText, param: new { Type = GlobalConstants.TABLE_TITLE }, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                    if (string.IsNullOrEmpty(voucherNo))
                    {
                        response.status = StatusCodes.Status204NoContent;
                        response.message = MessageConstants.MESSAGE_VOUCHER_NO_MISSING;
                        return response;
                    }
                    entity.Code = voucherNo;
                    entity.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                    entity.CreateDate = _dateTimeHelper.GetCurrentVietnamTime();
                    await _dbContext.Titles.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();
                    response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                }
                return response;
            }
            catch (Exception) { throw; }
        }


        /// <summary>
        /// Thêm cập nhật phòng ban
        /// </summary>
        /// <param name="process"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateTitle(Titles entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                var data = await _dbContext.Titles.FirstOrDefaultAsync(m => m.Id == entity.Id);
                if (data == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                data.Name = entity.Name;
                data.Remark = entity.Remark;
                data.IsActive = entity.IsActive;
                data.BranchId = entity.BranchId;
                data.DepartmentId = entity.DepartmentId;
                data.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                data.UpdateDate = _dateTimeHelper.GetCurrentVietnamTime();
                data.UserSign2 = entity.UserSign2;
                _dbContext.Titles.Attach(data);
                _dbContext.Entry(data).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
                return response;
            }
            catch (Exception) { throw; }
        }
        #endregion
    }
}
