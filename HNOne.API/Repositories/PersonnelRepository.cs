using Azure.Core;
using Dapper;
using HNOne.API.Constants;
using HNOne.API.Repositories.Interfaces;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Entities;
using HNOne.Model.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
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
                    entity.Id = await _dbContext.Employees.Select(m => m.Id).DefaultIfEmpty().MaxAsync() + 1;
                    entity.Code = voucherNo;
                    entity.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                    entity.CreateDate = _dateTimeHelper.GetCurrentVietnamTime();
                    await _dbContext.Employees.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();
                    response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                    response.data = entity.Id;
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
                data.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                data.UpdateDate = _dateTimeHelper.GetCurrentVietnamTime();
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
                _dbContext.Employees.Attach(data);
                _dbContext.Entry(data).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
                response.data = entity.Id;
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
                    DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
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
        #endregion
    }
}
