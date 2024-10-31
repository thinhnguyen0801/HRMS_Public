using Dapper;
using HNOne.API.Constants;
using HNOne.API.Repositories.Interfaces;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Entities;
using HNOne.Model.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Data;
using static Dapper.SqlMapper;

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
                parameters.Add("@UserId", request.userId, DbType.Int32);
                parameters.Add("@BranchId", request.branchId, DbType.Int32);
                parameters.Add("@StatusId", request.opt, DbType.String);
                var lstResult = await connection.QueryAsync<EmployeeModel>(StoreConstants.STORE_H1_EMPLOYEE_SELECT, param: parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.StoredProcedure);
                return lstResult;
            }; 
        }

        #endregion

        #region Command
        /// <summary>
        /// Thêm mới nhân viên
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> AddEmployee(Employees entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var connection = _dapperDbContext.CreateConnection())
                {
                    string commandText = @$"select {StoreConstants.FUNC_GET_VOUCHER}(@Type, '', '', '')";
                    string? voucherNo = await connection.QueryFirstOrDefaultAsync<string>(commandText, param: new { Type = GlobalConstants.TABLE_EMPLOYEE }, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                    if (string.IsNullOrEmpty(voucherNo))
                    {
                        response.status = StatusCodes.Status204NoContent;
                        response.message = MessageConstants.MESSAGE_VOUCHER_NO_MISSING;
                        return response;
                    }
                    entity.Id = await _dbContext.Employees.Select(m => m.Id).DefaultIfEmpty().MaxAsync() + 1;
                    entity.Code = voucherNo;
                    entity.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                    entity.CreateDate = _dateTimeHelper.GetCurrentVietnamTime();
                    await _dbContext.Employees.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();
                    response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                }
                return response;
            }
            catch (Exception) { throw; }

        }

        /// <summary>
        /// cập nhật nhân viên
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateEmployee(Employees entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                var data = await _dbContext.Employees.FirstOrDefaultAsync(m => m.Id == entity.Id);
                if (data == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                data.Name = entity.Name;
                data.DateOfBirth = entity.DateOfBirth;
                data.StatusId = entity.StatusId;
                data.Gender = entity.Gender;
                data.PlaceOfBirth = entity.PlaceOfBirth;
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
                data.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                data.UpdateDate = _dateTimeHelper.GetCurrentVietnamTime();
                data.UserSign2 = entity.UserSign2;
                _dbContext.Employees.Attach(data);
                _dbContext.Entry(data).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
                return response;
            }
            catch (Exception) { throw; }
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
                    DynamicParameters parameters = new DynamicParameters();
                    bool isResult = true;
                    isResult = await _dbContext.Contracts.FirstOrDefaultAsync(m => m.ContractCode == entity.ContractCode) != null;
                    if (isResult)
                    {
                        response.status = StatusCodes.Status409Conflict;
                        response.message = "Số hợp đồng đã tồn tại!";
                        return response;
                    }
                    entity.Id = await _dbContext.Contracts.Select(m => m.Id).DefaultIfEmpty().MaxAsync() + 1;
                    entity.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                    entity.CreateDate = _dateTimeHelper.GetCurrentVietnamTime();
                    await _dbContext.Database.BeginTransactionAsync();
                    isTrans = true;
                    await _dbContext.Contracts.AddAsync(entity);
                    // Thêm thông tin lương
                    if (!lstSalaryConfig.IsNullOrEmpty())
                    {
                        lstSalaryConfig = lstSalaryConfig!.Update(m =>
                        {
                            m.ContractId = entity.Id;
                            m.BranchId = entity.BranchId;
                            m.EmployeeId = entity.EmployeeId;
                        });
                        await _dbContext.SalaryAdjustments.AddRangeAsync(lstSalaryConfig!);
                    }    
                    await _dbContext.SaveChangesAsync();
                    await _dbContext.Database.CommitTransactionAsync();
                    response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                }
                return response;
            }
            catch (Exception)
            {
                if (isTrans) await _dbContext.Database.RollbackTransactionAsync();
                throw;
            }

        }

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
                data.TaxtTypeCode = entity.TaxtTypeCode;
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
                data.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                data.UpdateDate = _dateTimeHelper.GetCurrentVietnamTime();
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
                        _dbContext.SalaryAdjustments.Attach(item);
                        _dbContext.Entry(data).State = EntityState.Modified;
                    }    
                }
                await _dbContext.SaveChangesAsync();
                await _dbContext.Database.CommitTransactionAsync();
                response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
                return response;
            }
            catch (Exception)
            {
                if (isTrans) await _dbContext.Database.RollbackTransactionAsync();
                throw;
            }
        }
        #endregion
    }
}
