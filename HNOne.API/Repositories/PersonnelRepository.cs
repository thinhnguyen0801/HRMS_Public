using Azure.Core;
using Dapper;
using HNOne.API.Constants;
using HNOne.API.Repositories.Interfaces;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Entities;
using HNOne.Model.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using static Dapper.SqlMapper;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace HNOne.API.Repositories
{
    public class PersonnelRepository : IPersonnelRepository
    {
        private readonly MasterDbContext _dbContext;
        private readonly IDapperDbContext _dapperDbContext;
        private readonly IDateTimeHelper _dateTimeHelper;
        public PersonnelRepository(MasterDbContext dbContext
            , IDapperDbContext dapperDbContext, IDateTimeHelper dateTimeHelper)
        {
            _dbContext = dbContext;
            _dapperDbContext = dapperDbContext;
            _dateTimeHelper = dateTimeHelper;
        }

        #region Query

        /// <summary>
        /// lấy danh sách nhân viên
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IEnumerable<EmployeeModel>> GetEmployee(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@EmployeeId", request.employeeId, DbType.Int32);
                parameters.Add("@UserId", request.userId, DbType.Int32);
                parameters.Add("@BranchId", request.branchId, DbType.Int32);
                parameters.Add("@StatusId", request.opt, DbType.String);
                var lstResult = await connection.QueryAsync<EmployeeModel>(StoreConstants.STORE_H1_EMPLOYEE_SELECT, param: parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.StoredProcedure);
                return lstResult;
            }; 
        }

        /// <summary>
        /// lấy danh sách hợp đồng
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ContractModel>> GetContract(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                request.fromDate ??= new DateTime(2000, 01, 01);
                request.toDate ??= DateTime.Now.AddMonths(1);
                var parameters = new DynamicParameters();
                parameters.Add("@ContractId", request.documentId, DbType.Int32);
                parameters.Add("@UserId", request.userId, DbType.Int32);
                parameters.Add("@BranchId", request.branchId, DbType.Int32);
                parameters.Add("@StatusIds", request.opt, DbType.String);
                parameters.Add("@FromDate", request.fromDate, DbType.Date);
                parameters.Add("@ToDate", request.toDate, DbType.Date);
                parameters.Add("@EmployeeId", request.employeeId, DbType.Int32);
                IEnumerable<ContractModel>? lstResult = null;
                var dtResult = await connection.QueryMultipleAsync(StoreConstants.STORE_H1_CONTRACT_SELECT, param: parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.StoredProcedure);
                if(dtResult != null)
                {
                    lstResult = dtResult.Read<ContractModel>();
                    if(request.documentId > 0)
                    {
                        var lstSalaryConfig = dtResult.Read<SalaryConfigurationModel>();
                        string jsonDetail = JsonConvert.SerializeObject(lstSalaryConfig);
                        lstResult = lstResult.Update(m => m.jsonDetail = jsonDetail);
                    }    
                }    
                return lstResult ?? new List<ContractModel>();
            }    
        }

        /// <summary>
        /// lấy danh sách mối quan hệ gia đình
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public async Task<IEnumerable<FamilyRelationshipModel>> GetFamilyRelationship(int employeeId)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@EmployeeId", employeeId, DbType.Int32);
                string query = "select T0.*, T1.Name as RelationshipName " +
                    " from FamilyRelationships as T0 with(nolock)" +
                    " inner join EnumCatagories as T1 with(nolock) on T0.RelationshipId = T1.Code and T1.EnumType = 'QuanHeGiaDinh'" +
                    " where T0.IsDelete = 0 and T0.EmployeeId = @EmployeeId";
                var results = await connection.QueryAsync<FamilyRelationshipModel>(query, parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                return results;
            }
        }
        
        /// <summary>
        /// lấy danh sách bảo hiểm
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<InsuranceModel>> GetInsurance(int employeeId)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@EmployeeId", employeeId, DbType.Int32);
                string query = "select T0.*, T1.Name as InsuranceTypeName " +
                    " from Insurances as T0 with(nolock)" +
                    " inner join EnumCatagories as T1 with(nolock) on T0.InsuranceType = T1.Code and T1.EnumType = 'LoaiBaoHiem'" +
                    " where T0.IsDelete = 0 and T0.EmployeeId = @EmployeeId";
                var results = await connection.QueryAsync<InsuranceModel>(query, parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                return results;
            }    
        }

        /// <summary>
        /// lấy danh sách phụ lục hợp đồng
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ContractAppendixModel>> GetContractAppendix(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                int.TryParse(request.opt1, out int contractId);
                request.fromDate ??= new DateTime(2000, 01, 01);
                request.toDate ??= DateTime.Now.AddMonths(1);
                var parameters = new DynamicParameters();
                parameters.Add("@ContractAppendixId", request.documentId, DbType.Int32);
                parameters.Add("@ContractId", contractId, DbType.Int32);
                parameters.Add("@UserId", request.userId, DbType.Int32);
                parameters.Add("@BranchId", request.branchId, DbType.Int32);
                parameters.Add("@StatusIds", request.opt, DbType.String);
                parameters.Add("@FromDate", request.fromDate, DbType.Date);
                parameters.Add("@ToDate", request.toDate, DbType.Date);
                IEnumerable<ContractAppendixModel>? lstResult = null;
                var dtResult = await connection.QueryMultipleAsync(StoreConstants.STORE_H1_CONTRACT_APPENDIX_SELECT, param: parameters
                    , commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.StoredProcedure);
                if (dtResult != null)
                {
                    lstResult = dtResult.Read<ContractAppendixModel>();
                    if (request.documentId > 0)
                    {
                        var lstSalaryConfig = dtResult.Read<SalaryConfigurationModel>();
                        string jsonDetail = JsonConvert.SerializeObject(lstSalaryConfig);
                        lstResult = lstResult.Update(m => m.jsonDetail = jsonDetail);
                    }
                }
                return lstResult ?? new List<ContractAppendixModel>();
            }
        }

        public async Task<IEnumerable<LevelOfEducationModel>> GetEducation(int employeeId)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@EmployeeId", employeeId, DbType.Int32);
                string query = "select T0.*, T1.Name as RankingName " +
                    " from LevelOfEducations as T0 with(nolock)" +
                    " inner join EnumCatagories as T1 with(nolock) on T0.RankingCode = T1.Code and T1.EnumType = 'XepLoaiDaoTao'" +
                    " where T0.IsDelete = 0 and T0.EmployeeId = @EmployeeId";
                var results = await connection.QueryAsync<LevelOfEducationModel>(query, parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                return results;
            }
        }
        #endregion

        #region Command
        /// <summary>
        /// Thêm mới nhân viên
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> AddEmployee(Employees entity, bool isCreateAccount = false)
        {
            ResponseModel response = new ResponseModel();
            bool isTran = false;
            try
            {
                using (var connection = _dapperDbContext.CreateConnection())
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@Type", GlobalConstants.TABLE_EMPLOYEE, DbType.String);
                    parameters.Add("@Opt", entity.EmployeeType, DbType.String);
                    string commandText = @$"select {StoreConstants.FUNC_GET_VOUCHER}(@Type, @Opt, '', '')";
                    string? voucherNo = await connection.QueryFirstOrDefaultAsync<string>(commandText, param: parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                    if (string.IsNullOrEmpty(voucherNo))
                    {
                        response.status = StatusCodes.Status204NoContent;
                        response.message = MessageConstants.MESSAGE_VOUCHER_NO_MISSING;
                        return response;
                    }
                    DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                    entity.Id = await _dbContext.Employees.Select(m => m.Id).DefaultIfEmpty().MaxAsync() + 1;
                    entity.Code = voucherNo;
                    if (isCreateAccount)
                    {
                        Users account = new Users();
                        account.UserName = entity.Code; // lấy mã nhân nhân viên làm tên đăng nhập
                        account.EmployeeId = entity.Id;
                        bool isResult = true;
                        // lấy thông tin công ty
                        Branchs? branch = await _dbContext.Branchs.FirstOrDefaultAsync(m => m.BranchId == entity.BranchId);
                        if (branch == null || string.IsNullOrEmpty(branch.DefaultPassword))
                        {
                            response.status = StatusCodes.Status409Conflict;
                            response.message = $"Chi nhánh [{branch?.BranchCode}] chưa được cấu hình mật khẩu mặc định!";
                            return response;
                        }
                        // Tạo mới
                        isResult = await _dbContext.Users.AnyAsync(m => m.UserName == account.UserName && m.EmployeeId != account.EmployeeId);
                        if (isResult)
                        {
                            // nếu tên đăng nhập là mã số nhân viên đã được tạo
                            response.status = StatusCodes.Status409Conflict;
                            response.message = $"Tên đăng nhập [{account.UserName}] đã tồn tại!";
                            return response;
                        }
                        await _dbContext.Database.BeginTransactionAsync();
                        isTran = true;
                        account.UserId = await _dbContext.Users.Select(m => m.UserId).DefaultIfEmpty().MaxAsync() + 1;
                        account.BranchId = entity.BranchId;
                        account.Password = branch.DefaultPassword;
                        account.DefaultPassword = branch.DefaultPassword;
                        account.IsActive = true;
                        account.DepartmentIds = entity.DepartmentId > 0 ? entity.DepartmentId.ToString() : "";
                        account.BranchIds = entity.BranchId.ToString();
                        account.DateTracking = dateTimeNow;
                        account.CreateDate = dateTimeNow;
                        account.UserSign = entity.UserSign;
                        await _dbContext.Users.AddAsync(account);
                    }
                    entity.DateTracking = dateTimeNow;
                    entity.CreateDate = dateTimeNow;
                    await _dbContext.Employees.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();
                    if (isTran) await _dbContext.Database.CommitTransactionAsync();
                    response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                    response.data = entity.Id;
                }
                return response;
            }
            catch (Exception)
            {
                if (isTran) await _dbContext.Database.RollbackTransactionAsync();
                throw;
            }

        }

        /// <summary>
        /// cập nhật nhân viên
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateEmployee(Employees entity, bool isCreateAccount = false)
        {
            ResponseModel response = new ResponseModel();
            bool isTran = false;
            try
            {
                using (var connection = _dapperDbContext.CreateConnection())
                {
                    string strQuery = "select * from Employees as T0 with(nolock) where T0.IsDelete = 0 and Id = @EmployeeId";
                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@EmployeeId", entity.Id, DbType.Int32);
                    var data = await connection.QueryFirstOrDefaultAsync<Employees>(strQuery, parameters, commandTimeout: 500, commandType: CommandType.Text);
                    if (data == null)
                    {
                        response.status = StatusCodes.Status404NotFound;
                        response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                        return response;
                    }
                    if (isCreateAccount)
                    {
                        DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                        Users account = new Users();
                        account.UserName = data.Code; // lấy mã nhân nhân viên làm tên đăng nhập
                        account.EmployeeId = data.Id;
                        bool isResult = true;
                        // lấy thông tin công ty
                        Branchs? branch = await _dbContext.Branchs.FirstOrDefaultAsync(m => m.BranchId == data.BranchId);
                        if(branch == null || string.IsNullOrEmpty(branch.DefaultPassword))
                        {
                            response.status = StatusCodes.Status409Conflict;
                            response.message = $"Chi nhánh [{branch?.BranchCode}] chưa được cấu hình mật khẩu mặc định!";
                            return response;
                        }    
                        // Tạo mới
                        isResult = await _dbContext.Users.AnyAsync(m => m.UserName == account.UserName && m.EmployeeId != account.EmployeeId);
                        if (isResult)
                        {
                            // nếu tên đăng nhập là mã số nhân viên đã được tạo
                            response.status = StatusCodes.Status409Conflict;
                            response.message = $"Tên đăng nhập [{account.UserName}] đã tồn tại!";
                            return response;
                        }
                        parameters = new DynamicParameters();
                        parameters.Add("@EmployeeId", account.EmployeeId, DbType.Int32);
                        strQuery = "select T0.*, T2.[Code] as EmployeeCode, T2.[Name] as EmployeeName" +
                            " from Users as T0 with(nolock) " +
                            " inner join Employees as T2 with(nolock) on T0.EmployeeId = T2.Id" +
                            " where T0.IsDelete = 0 and T0.IsActive = 1 and T0.EmployeeId = @EmployeeId";
                        var result = await connection.QueryFirstOrDefaultAsync<UserModel>(strQuery, parameters, commandTimeout: 500, commandType: CommandType.Text);
                        if (result != null)
                        {
                            response.status = StatusCodes.Status409Conflict;
                            response.message = $"Nhân viên [{result.employeeCode}] đã thiết lập tài khoản [{result.userName}]!";
                            return response;
                        }
                        await _dbContext.Database.BeginTransactionAsync();
                        isTran = true;
                        account.UserId = await _dbContext.Users.Select(m => m.UserId).DefaultIfEmpty().MaxAsync() + 1;
                        account.BranchId = data.BranchId;
                        account.Password = branch.DefaultPassword;
                        account.DefaultPassword = branch.DefaultPassword;
                        account.IsActive = true;
                        account.DepartmentIds = data.DepartmentId > 0 ? data.DepartmentId.ToString() : "";
                        account.BranchIds = data.BranchId.ToString();
                        account.DateTracking = dateTimeNow;
                        account.CreateDate = dateTimeNow;
                        account.UserSign = entity.UserSign;
                        await _dbContext.Users.AddAsync(account);
                    }
                    employeeUpdate(ref data, entity);
                    _dbContext.Employees.Attach(data);
                    _dbContext.Entry(data).State = EntityState.Modified;
                    await _dbContext.SaveChangesAsync();
                    if (isTran) await _dbContext.Database.CommitTransactionAsync();
                    response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
                    response.data = entity.Id;
                    return response;
                }
            }
            catch (Exception)
            {
                if (isTran) await _dbContext.Database.RollbackTransactionAsync();
                throw;
            }
        }

        /// <summary>
        /// Thêm mới thông tin hợp đồng
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> AddContract(Contracts entity, IEnumerable<SalaryAdjustments>? lstSalaryConfig)
        {
            bool isTrans = false;
            ResponseModel response = new ResponseModel();
            try
            {
                using (var connection = _dapperDbContext.CreateConnection())
                {
                    DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                    DynamicParameters parameters = new DynamicParameters();
                    string strQuery = string.Empty;
                    bool isResult = true;   
                    isResult = await _dbContext.Contracts.AnyAsync(m => m.ContractCode == entity.ContractCode);
                    if (isResult)
                    {
                        response.status = StatusCodes.Status409Conflict;
                        response.message = "Số hợp đồng đã tồn tại!";
                        return response;
                    }
                    // kiểm tra có hợp đồng nào dỡ dang không
                    // nếu có thì không cho lưu
                    parameters.Add("@EmployeeId", entity.EmployeeId);
                    parameters.Add("@ContractTypeId", entity.ContractTypeId);
                    strQuery = "select top 1 T0.ContractCode as Code, T1.Name from Contracts as T0 with(nolock)" +
                        " inner join Employees as T1 with(nolock) on T0.EmployeeId = T1.Id" +
                        " where T0.IsDelete = 0 and T0.StatusCode in ('A', 'Y')" +
                        " and T0.EmployeeId = @EmployeeId and T0.ContractTypeId = @ContractTypeId";
                    var contractPending = await connection.QueryFirstOrDefaultAsync<ComboboxModel>(strQuery, parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                    if (contractPending != null)
                    {
                        response.message = $"Nhân viên [{contractPending.name}] đang có hợp đồng số [{contractPending.code}] chờ xử lý.";
                        response.status = StatusCodes.Status409Conflict;
                        return response;
                    }
                    entity.Id = await _dbContext.Contracts.Select(m => m.Id).DefaultIfEmpty().MaxAsync() + 1;
                    entity.DateTracking = dateTimeNow;
                    entity.CreateDate = dateTimeNow;
                    await _dbContext.Database.BeginTransactionAsync();
                    isTrans = true;
                    await _dbContext.Contracts.AddAsync(entity);
                    // Thêm thông tin lương
                    if (!lstSalaryConfig.IsNullOrEmpty())
                    {
                        lstSalaryConfig = lstSalaryConfig!.Update(m =>
                        {
                            m.Id = 0;
                            m.ContractId = entity.Id;
                            m.ContractAppendixId = -1;
                            m.BranchId = entity.BranchId;
                            m.EmployeeId = entity.EmployeeId;
                            m.UpdateDate = null;
                            m.CreateDate = dateTimeNow;
                            m.DateTracking = dateTimeNow;
                        });
                        await _dbContext.SalaryAdjustments.AddRangeAsync(lstSalaryConfig!);
                    }    
                    await _dbContext.SaveChangesAsync();
                    await _dbContext.Database.CommitTransactionAsync();
                    response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                    response.data = entity.Id; // nhã ra mã số hợp đồng
                }
                return response;
            }
            catch (Exception)
            {
                if (isTrans) await _dbContext.Database.RollbackTransactionAsync();
                throw;
            }

        }

        /// <summary>
        /// cập nhật thông tin phụ lục hợp đồng
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="lstSalaryConfig"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateContract(Contracts entity, IEnumerable<SalaryAdjustments>? lstSalaryConfig)
        {
            bool isTrans = false;
            ResponseModel response = new ResponseModel();
            try
            {
                var data = await _dbContext.Contracts.FirstOrDefaultAsync(m => m.Id == entity.Id);
                if (data == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                if(data.DateTracking != entity.DateTracking)
                {
                    response.status = StatusCodes.Status409Conflict;
                    response.message = MessageConstants.MESSAGE_DATA_CHECKING_MODIFIED;
                    return response;
                }    
                DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                data.EmployeeId = entity.EmployeeId;
                data.TimesheetId = entity.TimesheetId;
                data.StartDate = entity.StartDate;
                data.EndDate = entity.EndDate;
                data.DateOfSigning = entity.DateOfSigning;
                data.DeductionDate = entity.DeductionDate;
                data.EmployeeSignatureId = entity.EmployeeSignatureId;
                data.PositionId = entity.PositionId;
                data.TitleId = entity.TitleId;
                data.Remark = entity.Remark;
                data.StatusCode = entity.StatusCode;
                data.TaxTypeCode = entity.TaxTypeCode;
                data.SalaryCoefficient = entity.SalaryCoefficient;
                data.TotalSalary = entity.TotalSalary;
                data.NetSalary = entity.NetSalary;
                data.NumberOfMonths = entity.NumberOfMonths;
                data.NumberOfDaysReduced = entity.NumberOfDaysReduced;
                data.DecisionNo = entity.DecisionNo;
                data.PlaceOfWorkId = entity.PlaceOfWorkId;
                data.IsActive = entity.IsActive;
                data.IsCompanyDeduction = entity.IsCompanyDeduction;
                data.IsCompanyInsurance = entity.IsCompanyInsurance;
                data.DateTracking = dateTimeNow;
                data.UpdateDate = dateTimeNow;
                data.UserSign2 = entity.UserSign2;
                await _dbContext.Database.BeginTransactionAsync();
                isTrans = true;
                _dbContext.Contracts.Attach(data);
                _dbContext.Entry(data).State = EntityState.Modified;
                // Nếu có điều chỉnh lương
                if (!lstSalaryConfig.IsNullOrEmpty())
                {
                    foreach(var item in lstSalaryConfig!)
                    {
                        var dataSalary = await _dbContext.SalaryAdjustments.FirstOrDefaultAsync(m => m.Id == item.Id);
                        if (dataSalary == null) continue;
                        dataSalary.BranchId = entity.BranchId;
                        dataSalary.EmployeeId = entity.EmployeeId;
                        dataSalary.Amount = item.Amount;
                        dataSalary.SalaryCoefficient = item.SalaryCoefficient;
                        dataSalary.DateTracking = dateTimeNow;
                        dataSalary.UpdateDate = dateTimeNow;
                        dataSalary.UserSign2 = entity.UserSign2;
                        _dbContext.SalaryAdjustments.Attach(dataSalary);
                        _dbContext.Entry(dataSalary).State = EntityState.Modified;
                    }    
                }
                await _dbContext.SaveChangesAsync();
                await _dbContext.Database.CommitTransactionAsync();
                response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
                response.data = data.Id;
                return response;
            }
            catch (Exception)
            {
                if (isTrans) await _dbContext.Database.RollbackTransactionAsync();
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="lstSalaryConfig"></param>
        /// <returns></returns>
        public async Task<ResponseModel> AddContractAppendix(ContractAppendices entity, IEnumerable<SalaryAdjustments>? lstSalaryConfig)
        {
            bool isTrans = false;
            ResponseModel response = new ResponseModel();
            try
            {
                using (var connection = _dapperDbContext.CreateConnection())
                {
                    DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                    DynamicParameters parameters = new DynamicParameters();
                    bool isResult = true;
                    isResult = await _dbContext.ContractAppendices.FirstOrDefaultAsync(m => m.ContractCode == entity.ContractCode && m.ContractAppendixCode == entity.ContractAppendixCode) != null;
                    if (isResult)
                    {
                        response.status = StatusCodes.Status409Conflict;
                        response.message = "Số phụ lục hợp đồng đã tồn tại!";
                        return response;
                    }
                    entity.Id = await _dbContext.ContractAppendices.Select(m => m.Id).DefaultIfEmpty().MaxAsync() + 1;
                    entity.DateTracking = dateTimeNow;
                    entity.CreateDate = dateTimeNow;
                    await _dbContext.Database.BeginTransactionAsync();
                    isTrans = true;
                    await _dbContext.ContractAppendices.AddAsync(entity);
                    // Thêm thông tin lương
                    if (!lstSalaryConfig.IsNullOrEmpty())
                    {
                        lstSalaryConfig = lstSalaryConfig!.Update(m =>
                        {
                            m.Id = 0;
                            m.ContractId = entity.ContractId;
                            m.ContractAppendixId = entity.Id;
                            m.BranchId = entity.BranchId;
                            m.EmployeeId = entity.EmployeeId;
                            m.UpdateDate = null;
                            m.CreateDate = dateTimeNow;
                            m.DateTracking = dateTimeNow;
                        });
                        await _dbContext.SalaryAdjustments.AddRangeAsync(lstSalaryConfig!);
                    }
                    await _dbContext.SaveChangesAsync();
                    await _dbContext.Database.CommitTransactionAsync();
                    response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                    response.data = entity.Id; // nhã ra mã số phụ lục hợp đồng
                }
                return response;
            }
            catch (Exception)
            {
                if (isTrans) await _dbContext.Database.RollbackTransactionAsync();
                throw;
            }

        }

        public async Task<ResponseModel> UpdateContractAppendix(ContractAppendices entity, IEnumerable<SalaryAdjustments>? lstSalaryConfig)
        {
            bool isTrans = false;
            ResponseModel response = new ResponseModel();
            try
            {
                var data = await _dbContext.ContractAppendices.FirstOrDefaultAsync(m => m.Id == entity.Id);
                if (data == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                data.EmployeeId = entity.EmployeeId;
                data.TimesheetId = entity.TimesheetId;
                data.DateOfSigning = entity.DateOfSigning;
                data.EffectiveDate = entity.EffectiveDate;
                data.DeductionDate = entity.DeductionDate;
                data.EmployeeSignatureId = entity.EmployeeSignatureId;
                data.DepartmentId = entity.DepartmentId;
                data.PositionId = entity.PositionId;
                data.TitleId = entity.TitleId;
                data.Remark = entity.Remark;
                data.ContractNumber = entity.ContractNumber;
                data.StatusCode = entity.StatusCode;
                data.AuthorizationLetter = entity.AuthorizationLetter;
                data.IsSalaryAdjustment = entity.IsSalaryAdjustment;
                data.TaxTypeCode = entity.TaxTypeCode;
                data.SalaryCoefficient = entity.SalaryCoefficient;
                data.TotalSalary = entity.TotalSalary;
                data.NetSalary = entity.NetSalary;
                data.DecisionNo = entity.DecisionNo;
                data.PlaceOfWorkId = entity.PlaceOfWorkId;
                data.IsActive = entity.IsActive;
                data.IsCompanyDeduction = entity.IsCompanyDeduction;
                data.IsCompanyInsurance = entity.IsCompanyInsurance;
                data.DateTracking = dateTimeNow;
                data.UpdateDate = dateTimeNow;
                data.UserSign2 = entity.UserSign2;
                await _dbContext.Database.BeginTransactionAsync();
                isTrans = true;
                _dbContext.ContractAppendices.Attach(data);
                _dbContext.Entry(data).State = EntityState.Modified;
                // Nếu có điều chỉnh lương
                if (!lstSalaryConfig.IsNullOrEmpty())
                {
                    foreach (var item in lstSalaryConfig!)
                    {
                        var dataSalary = await _dbContext.SalaryAdjustments.FirstOrDefaultAsync(m => m.Id == item.Id);
                        if (dataSalary == null) continue;
                        dataSalary.BranchId = entity.BranchId;
                        dataSalary.EmployeeId = entity.EmployeeId;
                        dataSalary.Amount = item.Amount;
                        dataSalary.SalaryCoefficient = item.SalaryCoefficient;
                        dataSalary.DateTracking = dateTimeNow;
                        dataSalary.UpdateDate = dateTimeNow;
                        dataSalary.UserSign2 = entity.UserSign2;
                        _dbContext.SalaryAdjustments.Attach(dataSalary);
                        _dbContext.Entry(dataSalary).State = EntityState.Modified;
                    }
                }
                await _dbContext.SaveChangesAsync();
                await _dbContext.Database.CommitTransactionAsync();
                response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
                response.data = data.Id;
                return response;
            }
            catch (Exception)
            {
                if (isTrans) await _dbContext.Database.RollbackTransactionAsync();
                throw;
            }
        }

        /// <summary>
        /// Thêm quan hệ gia đình
        /// </summary>
        /// <param name="process"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> AddFamilyRelationship(FamilyRelationships entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                entity.DateTracking = dateTimeNow;
                entity.CreateDate = dateTimeNow;
                await _dbContext.FamilyRelationships.AddAsync(entity);
                await _dbContext.SaveChangesAsync();
                response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                return response;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// Cập nhật quan hệ gia đình
        /// </summary>
        /// <param name="process"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateFamilyRelationship(FamilyRelationships entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                var data = await _dbContext.FamilyRelationships.FirstOrDefaultAsync(m => m.Id == entity.Id);
                if (data == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                data.Name = entity.Name;
                data.EmployeeId = entity.EmployeeId;
                data.Name = entity.Name;
                data.RelationshipId = entity.RelationshipId;
                data.DateOfBirth = entity.DateOfBirth;
                data.PlaceOfBirth = entity.PlaceOfBirth;
                data.Occupation = entity.Occupation;
                data.PlaceOfOrigin = entity.PlaceOfOrigin;
                data.TemporaryAddress = entity.TemporaryAddress;
                data.ContactAddress = entity.ContactAddress;
                data.PhoneNumber = entity.PhoneNumber;
                data.CIC = entity.CIC;
                data.IssuanceDateCIC = entity.IssuanceDateCIC;
                data.Remark = entity.Remark;
                data.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                data.UpdateDate = _dateTimeHelper.GetCurrentVietnamTime();
                data.UserSign2 = entity.UserSign2;
                _dbContext.FamilyRelationships.Attach(data);
                _dbContext.Entry(data).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
                return response;
            }
            catch (Exception) { throw; }
        }
        
        /// <summary>
        /// Cập nhật thông tin hợp đồng
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateInsurance(string actionType, Insurances entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                bool isResult = true;
                if (actionType == ProcessConstants.POST_INSURANCE)
                {
                    // Tạo mới
                    isResult = await _dbContext.Insurances.FirstOrDefaultAsync(m => m.InsuranceNo == entity.InsuranceNo) != null;
                    if (isResult)
                    {
                        response.status = StatusCodes.Status409Conflict;
                        response.message = "Số bảo hiểm đã tồn tại!";
                        return response;
                    }
                    entity.Id = await _dbContext.Insurances.Select(m => m.Id).DefaultIfEmpty().MaxAsync() + 1;
                    entity.DateTracking = dateTimeNow;
                    entity.CreateDate = dateTimeNow;
                    await _dbContext.Insurances.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();
                    response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                    return response;
                }
                // cập nhật
                var data = await _dbContext.Insurances.FirstOrDefaultAsync(m => m.Id == entity.Id);
                if (data == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                isResult = await _dbContext.Insurances.FirstOrDefaultAsync(m => m.InsuranceNo == entity.InsuranceNo && m.Id != data.Id) != null;
                if (isResult)
                {
                    response.status = StatusCodes.Status409Conflict;
                    response.message = "Số bảo hiểm đã tồn tại!";
                    return response;
                }
                data.InsuranceType = entity.InsuranceType;
                data.InsuranceNo = entity.InsuranceNo;
                data.StartDate = entity.StartDate;
                data.EndDate = entity.EndDate;
                data.Rate = entity.Rate;
                data.ZipCode = entity.ZipCode;
                data.Address = entity.Address;
                data.AddressNo = entity.AddressNo;
                data.DateTracking = dateTimeNow;
                data.UpdateDate = dateTimeNow;
                data.UserSign2 = entity.UserSign2;
                _dbContext.Insurances.Attach(data);
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

        public async Task<ResponseModel> UpdateEducation(string actionType, LevelOfEducations entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                if (actionType == ProcessConstants.POST_EDUCATION)
                {
                    // Tạo mới
                    entity.Id = await _dbContext.LevelOfEducations.Select(m => m.Id).DefaultIfEmpty().MaxAsync() + 1;
                    entity.DateTracking = dateTimeNow;
                    entity.CreateDate = dateTimeNow;
                    await _dbContext.LevelOfEducations.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();
                    response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                    return response;
                }
                // cập nhật
                var data = await _dbContext.LevelOfEducations.FirstOrDefaultAsync(m => m.Id == entity.Id);
                if (data == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                data.FromYear = entity.FromYear;
                data.ToYear = entity.ToYear;
                data.LevelOfEducation = entity.LevelOfEducation;
                data.EducationalInstitution1 = entity.EducationalInstitution1;
                data.EducationalInstitution2 = entity.EducationalInstitution2;
                data.MajorCode = entity.MajorCode;
                data.RankingCode = entity.RankingCode;
                data.RankingName = entity.RankingName;
                data.IsComplete = entity.IsComplete;
                data.DateTracking = dateTimeNow;
                data.UpdateDate = dateTimeNow;
                data.UserSign2 = entity.UserSign2;
                _dbContext.LevelOfEducations.Attach(data);
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
        
        /// <summary>
        /// kiểm tra dữ liệu trước khi lưu
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseModel> CheckExistsData(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                ResponseModel response = new ResponseModel();
                var parameters = new DynamicParameters();
                string query = "";
                switch(request.process)
                {
                    case ProcessConstants.POST_CONTRACT:
                        parameters.Add("@EmployeeId", request.employeeId);
                        parameters.Add("@ContractTypeId", request.type);
                        parameters.Add("@StatusCode", CommonConstants.STATUS_CODE_APPROVED); // kiểm tra đã duyệt chưa
                        parameters.Add("@FromDate", request.fromDate); // kiểm tra đã duyệt chưa
                        query = "select top 1 T0.ContractCode as Code, T1.Name from Contracts as T0 with(nolock) " +
                            " inner join Employees as T1 with(nolock) on T0.EmployeeId = T1.Id" +
                            " where T0.IsDelete = 0 and T0.StatusCode = @StatusCode" + // tình trạng là đã duyệt
                            " and cast(@FromDate as date) <= cast(T0.EndDate as date)" + // ngày bắt đầu nhỏ hơn ngày kết thúc
                            " and T0.EmployeeId = @EmployeeId and T0.ContractTypeId = @ContractTypeId";
                        var contract = await connection.QueryFirstOrDefaultAsync<ComboboxModel>(query, parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                        if(contract != null)
                        {
                            response.message = $"Nhân viên [ {contract.name} ] đang áp dụng hợp đồng số [ {contract.code} ]. <br />" +
                                $"Bạn có muốn thay thế bằng hợp đồng hiện tại?";
                            response.status = StatusCodes.Status409Conflict;
                        }    
                        break;
                    default:
                        response.status = StatusCodes.Status404NotFound;
                        response.message = $"Process Key {request.process} was not provider!!!";
                        break;

                }
                return response;
            }    
        }
        #endregion

        #region Private Functions

        /// <summary>
        /// gán dữ liệu nhân viên update
        /// </summary>
        /// <param name="data"></param>
        /// <param name="entity"></param>
        private void employeeUpdate(ref Employees data, Employees entity)
        {
            DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
            data.Name = entity.Name;
            data.DateOfBirth = entity.DateOfBirth;
            data.IsOnlybirthYear = entity.IsOnlybirthYear;
            data.StatusId = entity.StatusId;
            data.Gender = entity.Gender;
            data.PlaceOfOrigin = entity.PlaceOfOrigin;
            data.TemporaryAddress = entity.TemporaryAddress;
            data.ContactAddress = entity.ContactAddress;
            data.Religion = entity.Religion;
            data.Ethnicity = entity.Ethnicity;
            data.ImageUrl = entity.ImageUrl;
            data.MaritalStatus = entity.MaritalStatus;
            data.DateOfJoining = entity.DateOfJoining;
            data.StartDate = entity.StartDate;
            data.Remark = entity.Remark;
            data.CIC = entity.CIC;
            data.IssuanceDateCIC = entity.IssuanceDateCIC;
            data.PlaceOfIssuanceCIC = entity.PlaceOfIssuanceCIC;
            data.ExpiryDateCIC = entity.ExpiryDateCIC;
            data.Phone1 = entity.Phone1;
            data.Phone2 = entity.Phone2;
            data.Phone3 = entity.Phone3;
            data.Email1 = entity.Email1;
            data.Email2 = entity.Email2;
            data.AccountNumber = entity.AccountNumber;
            data.BankName = entity.BankName;
            data.BankBranch = entity.BankBranch;
            data.Beneficiary = entity.Beneficiary;
            data.Nationality = entity.Nationality;
            data.TaxNumber = entity.TaxNumber;
            data.LevelOfEducationId1 = entity.LevelOfEducationId1;
            data.LevelOfEducationId2 = entity.LevelOfEducationId2;
            data.MajorId1 = entity.MajorId1;
            data.MajorId2 = entity.MajorId2;
            data.EducationalInstitution1 = entity.EducationalInstitution1;
            data.EducationalInstitution2 = entity.EducationalInstitution2;
            data.Ranking1 = entity.Ranking1;
            data.Ranking2 = entity.Ranking2;
            data.LanguageLevel = entity.LanguageLevel;
            data.LevelOfComputerLiteracy = entity.LevelOfComputerLiteracy;
            data.OtherSkills = entity.OtherSkills;
            data.ProbationEndDate = entity.ProbationEndDate;
            data.BranchId = entity.BranchId;
            data.DepartmentId = entity.DepartmentId;
            data.PositionId = entity.PositionId;
            data.TitleId = entity.TitleId;
            data.ManagerId = entity.ManagerId;
            data.AttendanceSheetId = entity.AttendanceSheetId;
            data.DateTracking = dateTimeNow;
            data.UpdateDate = dateTimeNow;
            data.UserSign2 = entity.UserSign2;
            data.PassportNumber = entity.PassportNumber;
            data.IssueDatePassport = entity.IssueDatePassport;
            data.PlaceOfIssuePassport = entity.PlaceOfIssuePassport;
            data.ExpiryDatePassport = entity.ExpiryDatePassport;
            data.GraduationYear = entity.GraduationYear;
            data.Phone4 = entity.Phone4;
            data.Email3 = entity.Email3;
            data.ProvinceCode = entity.ProvinceCode;
            data.ProvinceName = entity.ProvinceName;
            data.PlaceOfBirth = entity.PlaceOfBirth;
            data.CountryCode1 = entity.CountryCode1;
            data.CountryName1 = entity.CountryName1;
            data.ProvinceCode1 = entity.ProvinceCode1;
            data.ProvinceName1 = entity.ProvinceName1;
            data.DistrictCode1 = entity.DistrictCode1;
            data.DistrictName1 = entity.DistrictName1;
            data.WardCode1 = entity.WardCode1;
            data.WardName1 = entity.WardName1;
            data.HouseNumber1 = entity.HouseNumber1;
            data.PlaceOfResidence = entity.PlaceOfResidence;
            data.HouseholdRegistrationNumber = entity.HouseholdRegistrationNumber;
            data.HouseholdNumber = entity.HouseholdNumber;
            data.CountryCode2 = entity.CountryCode2;
            data.CountryName2 = entity.CountryName2;
            data.ProvinceCode2 = entity.ProvinceCode2;
            data.ProvinceName2 = entity.ProvinceName2;
            data.DistrictCode2 = entity.DistrictCode2;
            data.DistrictName2 = entity.DistrictName2;
            data.WardCode2 = entity.WardCode2;
            data.WardName2 = entity.WardName2;
            data.HouseNumber2 = entity.HouseNumber2;
            data.FullName1 = entity.FullName1;
            data.Relationship = entity.Relationship;
            data.Phone5 = entity.Phone5;
            data.Phone6 = entity.Phone6;
            data.Email4 = entity.Email4;
            data.LevelCode = entity.LevelCode;
            data.GradeCode = entity.GradeCode;
            data.TraineeDate = entity.TraineeDate;
            data.ProbationStartDate = entity.ProbationStartDate;
            data.ManagerId2 = entity.ManagerId2;
            data.AttendanceSheetCode = entity.AttendanceSheetCode;
            data.ShiftCode = entity.ShiftCode; // ca làm việc
        }
        #endregion
    }
}
