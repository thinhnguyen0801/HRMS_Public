using Azure.Core;
using Dapper;
using HNOne.API.Constants;
using HNOne.API.Repositories.Interfaces;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Entities;
using HNOne.Model.Models;
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
            var lstBranch = await _dbContext.Branchs.Where(m=> !m.IsDelete).ToListAsync();
            return lstBranch;
        }

        public async Task<IEnumerable<Menus>> GetMenu()
        {
            var lstMenus = await _dbContext.Menus.Where(m => m.IsVisible).ToListAsync();
            return lstMenus;
        }

        public async Task<IEnumerable<Departments>> GetDepartment(RequestModel request)
        {
            var lstData = await _dbContext.Departments.Where(m => !m.IsDelete).ToListAsync();
            return lstData;
        }
        public async Task<IEnumerable<Titles>> GetTitle(RequestModel request)
        {
            var lstData = await _dbContext.Titles.Where(m => !m.IsDelete).ToListAsync();
            return lstData;
        }
        public async Task<IEnumerable<Positions>> GetPosition(RequestModel request)
        {
            var lstData = await _dbContext.Positions.Where(m => !m.IsDelete).ToListAsync();
            return lstData;
        }
        public async Task<IEnumerable<ContractTypes>> GetContractType(RequestModel request)
        {
            var lstData = await _dbContext.ContractTypes.Where(m => !m.IsDelete).ToListAsync();
            return lstData;
        }
        public async Task<IEnumerable<ReasonCategories>> GetReasonCategorie(RequestModel request)
        {
            var lstData = await _dbContext.ReasonCategories.Where(m => !m.IsDelete).ToListAsync();
            return lstData;
        }

        /// <summary>
        /// lấy danh sách enum
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public async Task<IEnumerable<EnumCatagories>> GetEnum(string enumType)
        {
            var lstEnums = await _dbContext.EnumCatagories.Where(m => m.EnumType == enumType).OrderBy(m=> m.RowOrder).ToListAsync();
            return lstEnums;
        }

        /// <summary>
        /// lấy danh sách loại lương
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public async Task<IEnumerable<SalaryCategories>> GetSalaryCatagory(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                string query = "select T0.* from SalaryCategories as T0 with(nolock)" +
                    " where T0.IsDelete = '0'";
                // thêm điều kiện
                if (request.opt == "ACTIVE") query += " and T0.IsActive = '1'";
                query += " order by T0.RowOrder";
                var results = await connection.QueryAsync<SalaryCategories>(query, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                return results;
            }
        }

        /// <summary>
        /// lấy danh sách cấu hình lương
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<SalaryConfigurationModel>> GetSalarySalaryConfig()
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                string query = "select T0.*, T1.Code as SalaryCategoryCode" +
                    ",T1.Name as SalaryCategoryName" +
                    ",T2.BranchCode, T2.BranchName, T3.Name as SalaryCalculateMethodName" +
                    " from SalaryConfigurations as T0 with(nolock) " +
                    " inner join SalaryCategories as T1 with(nolock) on T0.SalaryCategoryId = T1.Id " +
                    " inner join Branchs as T2 with(nolock) on T0.BranchId = T2.BranchId" +
                    " left join EnumCatagories as T3 with(nolock) on T0.SalaryCalculateMethod = T3.Code and T3.EnumType = 'CachTinhLuongPhuCap'";
                //var parameters = new DynamicParameters();
                //parameters.Add("@EmployeeId", request.employeeId, DbType.Int32);
                var results = await connection.QueryAsync<SalaryConfigurationModel>(query, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                return results;
            };
        }
        
        /// <summary>
        /// lấy mã chứng từ
        /// </summary>
        /// <param name="type"></param>
        /// <param name="opt"></param>
        /// <param name="opt1"></param>
        /// <param name="opt2"></param>
        /// <returns></returns>
        public async Task<string?> GetDocumentNo(string? type, string? opt = "", string? opt1 = "", string? opt2 = "")
        {
            try
            {
                using (var connection = _dapperDbContext.CreateConnection())
                {
                    string commandText = @$"select {StoreConstants.FUNC_GET_VOUCHER}(@Type,@Opt,@Opt1,@Opt2)";
                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@Type", type, DbType.String);
                    parameters.Add("@Opt", opt, DbType.String);
                    parameters.Add("@Opt1", opt1, DbType.String);
                    parameters.Add("@Opt2", opt2, DbType.String);
                    string? voucherNo = await connection.QueryFirstOrDefaultAsync<string>(commandText, param: parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                    return voucherNo;
                }    
            }
            catch (Exception) { throw; }
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
                    string commandText = @$"select {StoreConstants.FUNC_GET_VOUCHER}(@Type, '', '', '')";
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
                entity.Id = await _dbContext.Departments.Select(m => m.Id).DefaultIfEmpty().MaxAsync() + 1;
                entity.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                entity.CreateDate = _dateTimeHelper.GetCurrentVietnamTime();
                await _dbContext.Departments.AddAsync(entity);
                await _dbContext.SaveChangesAsync();
                response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
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
                    string commandText = @$"select {StoreConstants.FUNC_GET_VOUCHER}(@Type, '', '', '')";
                    string? voucherNo = await connection.QueryFirstOrDefaultAsync<string>(commandText, param: new { Type = GlobalConstants.TABLE_POSITION }, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                    if (string.IsNullOrEmpty(voucherNo))
                    {
                        response.status = StatusCodes.Status204NoContent;
                        response.message = MessageConstants.MESSAGE_VOUCHER_NO_MISSING;
                        return response;
                    }
                    entity.Id = await _dbContext.Positions.Select(m => m.Id).DefaultIfEmpty().MaxAsync() + 1;
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
                    string commandText = @$"select {StoreConstants.FUNC_GET_VOUCHER}(@Type, '', '', '')";
                    string? voucherNo = await connection.QueryFirstOrDefaultAsync<string>(commandText, param: new { Type = GlobalConstants.TABLE_TITLE }, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                    if (string.IsNullOrEmpty(voucherNo))
                    {
                        response.status = StatusCodes.Status204NoContent;
                        response.message = MessageConstants.MESSAGE_VOUCHER_NO_MISSING;
                        return response;
                    }
                    entity.Id = await _dbContext.Titles.Select(m => m.Id).DefaultIfEmpty().MaxAsync() + 1;
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

        /// <summary>
        /// Thêm loại hợp đồng
        /// </summary>
        /// <param name="process"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> AddContractType(ContractTypes entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                entity.Id = await _dbContext.ContractTypes.Select(m => m.Id).DefaultIfEmpty().MaxAsync() + 1;
                entity.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                entity.CreateDate = _dateTimeHelper.GetCurrentVietnamTime();
                await _dbContext.ContractTypes.AddAsync(entity);
                await _dbContext.SaveChangesAsync();
                response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                return response;
            }
            catch (Exception) { throw; }
        }


        /// <summary>
        /// Thêm cập nhật loại hợp đồng
        /// </summary>
        /// <param name="process"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateContractType(ContractTypes entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                var data = await _dbContext.ContractTypes.FirstOrDefaultAsync(m => m.Id == entity.Id);
                if (data == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                data.Name = entity.Name;
                data.Remark = entity.Remark;
                data.BranchId = entity.BranchId;
                data.StatusCode = entity.StatusCode;
                data.Duration = entity.Duration;
                data.IsIndefiniteDuration = entity.IsIndefiniteDuration;
                data.NumberOfDaysReduced = entity.NumberOfDaysReduced;
                data.IsActive = entity.IsActive;
                data.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                data.UpdateDate = _dateTimeHelper.GetCurrentVietnamTime();
                data.UserSign2 = entity.UserSign2;
                _dbContext.ContractTypes.Attach(data);
                _dbContext.Entry(data).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
                return response;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// Thêm danh mục lý do
        /// </summary>
        /// <param name="process"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> AddReasonCategorie(ReasonCategories entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var connection = _dapperDbContext.CreateConnection())
                {
                    entity.Id = await _dbContext.ReasonCategories.Select(m => m.Id).DefaultIfEmpty().MaxAsync() + 1;
                    entity.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                    entity.CreateDate = _dateTimeHelper.GetCurrentVietnamTime();
                    await _dbContext.ReasonCategories.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();
                    response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                }
                return response;
            }
            catch (Exception) { throw; }
        }


        /// <summary>
        /// Thêm cập nhật loại hợp đồng
        /// </summary>
        /// <param name="process"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateReasonCategorie(ReasonCategories entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                var data = await _dbContext.ReasonCategories.FirstOrDefaultAsync(m => m.Id == entity.Id);
                if (data == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                data.Name = entity.Name;
                data.Type = entity.Type;
                data.IsActive = entity.IsActive;
                data.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                data.UpdateDate = _dateTimeHelper.GetCurrentVietnamTime();
                data.UserSign2 = entity.UserSign2;
                _dbContext.ReasonCategories.Attach(data);
                _dbContext.Entry(data).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
                return response;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// Thêm mới danh mục loại lương
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> AddSalaryCategory(SalaryCategories entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var connection = _dapperDbContext.CreateConnection())
                {
                    DynamicParameters parameters = new DynamicParameters();
                    bool isResult = true;
                    string strQuery = "select count(1) from [SalaryCategories] with(nolock) where Code = @Code";
                    isResult = await connection.ExecuteScalarAsync<bool>(strQuery, new { Code = entity.Code?.Trim() });
                    if (isResult)
                    {
                        response.status = StatusCodes.Status409Conflict;
                        response.message = "Mã loại lương đã tồn tại!";
                        return response;
                    }
                    entity.Id = await _dbContext.SalaryCategories.Select(m => m.Id).DefaultIfEmpty().MaxAsync() + 1;
                    entity.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                    entity.CreateDate = _dateTimeHelper.GetCurrentVietnamTime();
                    await _dbContext.SalaryCategories.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();
                    response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                    return response;
                }    
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// cập nhật thông tin loại lương
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateSalaryCategory(SalaryCategories entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                var result = await _dbContext.SalaryCategories.FirstOrDefaultAsync(m => m.Id == entity.Id);
                if (result == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                result.Name = entity.Name;
                result.RowOrder = entity.RowOrder;
                result.IsActive = entity.IsActive;
                result.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                result.UpdateDate = _dateTimeHelper.GetCurrentVietnamTime();
                result.UserSign2 = entity.UserSign2;
                _dbContext.SalaryCategories.Attach(result);
                _dbContext.Entry(result).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
                return response;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// lưu thông tin cấu hình lương
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> AddSalaryConfig(SalaryConfigurations entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var connection = _dapperDbContext.CreateConnection())
                {
                    DynamicParameters parameters = new DynamicParameters();
                    bool isResult = true;
                    string strQuery = "select count(1) from [SalaryConfigurations] with(nolock) where SalaryCategoryId = @SalaryCategoryId and BranchId = @BranchId";
                    isResult = await connection.ExecuteScalarAsync<bool>(strQuery, new { entity.SalaryCategoryId, entity.BranchId });
                    if (isResult)
                    {
                        response.status = StatusCodes.Status409Conflict;
                        response.message = "Thông tin cấu hình lương đã tồn tại. Vui lòng kiểm tra Loại lương và Chi nhánh!";
                        return response;
                    }
                    entity.Id = await _dbContext.SalaryConfigurations.Select(m => m.Id).DefaultIfEmpty().MaxAsync() + 1;
                    entity.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                    entity.CreateDate = _dateTimeHelper.GetCurrentVietnamTime();
                    await _dbContext.SalaryConfigurations.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();
                    response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                    return response;
                }
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// cập nhật thông tin cấu hình tính lương
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateSalaryConfig(SalaryConfigurations entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                var result = await _dbContext.SalaryConfigurations.FirstOrDefaultAsync(m => m.Id == entity.Id);
                if (result == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                result.IsActive = entity.IsActive;
                result.IsPersonalIncomeTax = entity.IsPersonalIncomeTax;
                result.TaxLimit = entity.TaxLimit;
                result.IsSocialInsurance = entity.IsSocialInsurance;
                result.IsHealthInsurance = entity.IsHealthInsurance;
                result.IsAccidentInsurance = entity.IsAccidentInsurance;
                result.IsOccupationalAccidentInsurance = entity.IsOccupationalAccidentInsurance;
                result.IsUnionFee = entity.IsUnionFee;
                result.IsOvertime = entity.IsOvertime;
                result.OvertimeCoefficient = entity.OvertimeCoefficient;
                result.IsNightShift = entity.IsNightShift;
                result.CoefficientNightShift = entity.CoefficientNightShift;
                result.IsAllowance = entity.IsAllowance;
                result.IsProbationaryPeriod = entity.IsProbationaryPeriod;
                result.SalaryDefault = entity.SalaryDefault;
                result.SalaryCalculateMethod = entity.SalaryCalculateMethod;
                result.IsUseOfGradeLevel = entity.IsUseOfGradeLevel;
                result.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                result.UpdateDate = _dateTimeHelper.GetCurrentVietnamTime();
                result.UserSign2 = entity.UserSign2;
                _dbContext.SalaryConfigurations.Attach(result);
                _dbContext.Entry(result).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
                return response;
            }
            catch (Exception) { throw; }
        }
        #endregion
    }
}
