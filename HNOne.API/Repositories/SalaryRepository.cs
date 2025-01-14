using HNOne.API.Repositories.Interfaces;
using HNOne.Common;
using HNOne.Model.Entities;
using HNOne.Model;
using Microsoft.EntityFrameworkCore;
using HNOne.Model.Models;
using Dapper;
using HNOne.API.Constants;
using System.Data;
using Newtonsoft.Json;
using Azure;
using System.Text.RegularExpressions;

namespace HNOne.API.Repositories
{
    public class SalaryRepository : ISalaryRepository
    {
        private readonly MasterDbContext _dbContext;
        private readonly IDapperDbContext _dapperDbContext;
        private readonly IDateTimeHelper _dateTimeHelper;

        public SalaryRepository(MasterDbContext dbContext
            , IDapperDbContext dapperDbContext, IDateTimeHelper dateTimeHelper)
        {
            _dbContext = dbContext;
            _dapperDbContext = dapperDbContext;
            _dateTimeHelper = dateTimeHelper;
        }

        #region Query
        /// <summary>
        /// lấy dữ liệu tính lương của nhân viên
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IEnumerable<PayrollModel>> GetMonthlySalary(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                int.TryParse(request.opt, out int year);
                int.TryParse(request.opt1, out int month);
                if (year == 0) year = DateTime.Now.Year;
                if (month == 0) month = DateTime.Now.Month;
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", request.userId);
                parameters.Add("@BranchId", request.branchId);
                parameters.Add("@Year", year);
                parameters.Add("@Month", month);
                parameters.Add("@DepartmentIds", $"{request.departmentIds}");
                parameters.Add("@StatusIds", request.opt2);
                parameters.Add("@EmployeeIds", request.opt3);
                parameters.Add("@Type", $"{request.type}");
                var lstResult = await connection.QueryAsync<PayrollModel>(StoreConstants.STORE_H1_PAYROLL_CALCULATION_SELECT, param: parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.StoredProcedure);
                return lstResult;
            }
        }
        #endregion Query

        #region Command

        /// <summary>
        /// lưu bảng lương
        /// </summary>
        /// <param name="isLocked"></param>
        /// <param name="userId"></param>
        /// <param name="lstEntity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdatePayroll(bool isLocked, int userId, IEnumerable<Payrolls> lstEntity)
        {
            ResponseModel response = new ResponseModel();
            bool isTrans = false;
            try
            {
                DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                // Tạo mới
                await _dbContext.Database.BeginTransactionAsync();
                isTrans = true;
                foreach (var entity in lstEntity)
                {
                    var data = await _dbContext.Payrolls.FirstOrDefaultAsync(m => m.EmployeeId == entity.EmployeeId && m.Month == entity.Month && m.Year == entity.Year);
                    if(data == null)
                    {
                        entity.CreateDate = dateTimeNow;
                        entity.DateTracking = dateTimeNow;
                        entity.UserSign = userId;
                        entity.IsLocked = isLocked;
                        await _dbContext.Payrolls.AddAsync(entity);
                        continue;
                    }
                    if (data.IsLocked)
                    {
                        response.status = StatusCodes.Status409Conflict;
                        response.message = $"Nhân viên {data.EmployeeCode} đã được khóa kỳ dữ liệu lương!!!";
                        if (isTrans) await _dbContext.Database.RollbackTransactionAsync();
                        return response;
                    }
                    // cập nhật dữ liệu
                    data.BranchId = entity.BranchId;
                    #region Dữ liệu công của nhân viên
                    data.AttendanceSummaryId = entity.AttendanceSummaryId;
                    data.TNC = entity.TNC;
                    data.CDM = entity.CDM;
                    data.GCDM = entity.GCDM;
                    data.CTT = entity.CTT;
                    data.NL = entity.NL;
                    data.NPN = entity.NPN;
                    data.NCD = entity.NCD;
                    data.NPKL = entity.NPKL;
                    data.NB = entity.NB;
                    data.NKP = entity.NKP;
                    data.CTPC = entity.CTPC;
                    data.TGDLTVS = entity.TGDLTVS;
                    data.SLDLTVS = entity.SLDLTVS;
                    data.SGT = entity.SGT;
                    data.SGTC = entity.SGTC;
                    data.GCTC = entity.GCTC;
                    data.TGTC = entity.TGTC;
                    data.SGTCTC = entity.SGTCTC;
                    data.SGTCTT = entity.SGTCTT;
                    data.SGTCKT = entity.SGTCKT;
                    #endregion

                    #region Thông tin hợp đồng & lương
                    data.ContractId = entity.ContractId;
                    data.ContractCode = entity.ContractCode;
                    data.ContractTypeId = entity.ContractTypeId;
                    data.ContractTypeName = entity.ContractTypeName;
                    data.ContractAppendixId = entity.ContractAppendixId;
                    data.ContractAppendixCode = entity.ContractAppendixCode;
                    data.IsCompanyDeduction = entity.IsCompanyDeduction;
                    data.IsCompanyInsurance = entity.IsCompanyInsurance;
                    data.BasicSalary = entity.BasicSalary;
                    data.NegotiatedSalary = entity.NegotiatedSalary;
                    data.SalaryCoefficient = entity.SalaryCoefficient;
                    data.ConvertedNegotiatedSalary = entity.ConvertedNegotiatedSalary;
                    data.AllowanceSalary = entity.AllowanceSalary;
                    data.TotalSalaryGross = entity.TotalSalaryGross;
                    data.TotalSalaryNet = entity.TotalSalaryNet;
                    data.OvertimeSalary = entity.OvertimeSalary;
                    data.ActualSalary = entity.ActualSalary;
                    data.AnnualLeaveSalary = entity.AnnualLeaveSalary;
                    data.RegulatedSalary = entity.RegulatedSalary;
                    data.HolidaySalary = entity.HolidaySalary;
                    data.MissingWorkingHourSalary = entity.MissingWorkingHourSalary;
                    data.LateSalary = entity.LateSalary;
                    #endregion

                    #region Thông tin về trích nộp
                    data.ContributionSalarySI = entity.ContributionSalarySI;
                    data.DeductionEnterpriseSI = entity.DeductionEnterpriseSI;
                    data.DeductionEmployeeSI = entity.DeductionEmployeeSI;
                    data.ContributionSalaryHI = entity.ContributionSalaryHI;
                    data.DeductionEnterpriseHI = entity.DeductionEnterpriseHI;
                    data.DeductionEmployeeHI = entity.DeductionEmployeeHI;
                    data.ContributionSalaryUI = entity.ContributionSalaryUI;
                    data.DeductionEnterpriseUI = entity.DeductionEnterpriseUI;
                    data.DeductionEmployeeUI = entity.DeductionEmployeeUI;
                    data.ContributionSalaryAI = entity.ContributionSalaryAI;
                    data.DeductionEnterpriseAI = entity.DeductionEnterpriseAI;
                    data.DeductionEmployeeAI = entity.DeductionEmployeeAI;
                    data.TotalDeductionEnterprise = entity.TotalDeductionEnterprise;
                    data.TotalDeductionEmployee = entity.TotalDeductionEmployee;
                    data.TotalDeduction = entity.TotalDeduction;
                    data.UnionFeeSalary = entity.UnionFeeSalary;
                    data.DeductionEnterpriseUF = entity.DeductionEnterpriseUF;
                    data.DeductionEmployeeUF = entity.DeductionEmployeeUF;
                    #endregion

                    #region Thuế thu nhập cá nhân
                    data.SalaryParameterId = entity.SalaryParameterId;
                    data.TaxTypeCode = entity.TaxTypeCode;
                    data.TaxTypeName = entity.TaxTypeName;
                    data.TaxtRateId = entity.TaxtRateId;
                    data.TaxBracket = entity.TaxBracket;
                    data.MinTaxSalary = entity.MinTaxSalary;
                    data.MaxTaxSalary = entity.MaxTaxSalary;
                    data.TaxRate = entity.TaxRate;
                    data.ProgressiveAmount = entity.ProgressiveAmount;
                    data.StandardTax = entity.StandardTax;
                    data.FamilyCircumstanceTaxDeduction = entity.FamilyCircumstanceTaxDeduction;
                    data.NumOfPeopleTaxFCTaxDeduction = entity.NumOfPeopleTaxFCTaxDeduction;
                    data.TotalFCTaxDeduction = entity.TotalFCTaxDeduction;
                    data.TaxableIncome = entity.TaxableIncome;
                    data.TaxAllowance = entity.TaxAllowance;
                    data.TaxPayment = entity.TaxPayment;
                    #endregion

                    data.IsLocked = isLocked;
                    data.DateTracking = dateTimeNow;
                    data.UpdateDate = dateTimeNow;
                    data.UserSign2 = userId;

                    _dbContext.Payrolls.Attach(data);
                    _dbContext.Entry(data).State = EntityState.Modified;
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
        
        /// <summary>
        /// mở khóa kỳ lương cho nhân viên
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="lstEntity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UnLockPayroll(int userId, IEnumerable<Payrolls> lstEntity)
        {
            ResponseModel response = new ResponseModel();
            bool isTrans = false;
            try
            {
                DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                // Tạo mới
                await _dbContext.Database.BeginTransactionAsync();
                isTrans = true;
                foreach (var entity in lstEntity)
                {
                    var data = await _dbContext.Payrolls.FirstOrDefaultAsync(m => m.EmployeeId == entity.EmployeeId && m.Month == entity.Month && m.Year == entity.Year);
                    if(data == null || !data.IsLocked)
                    {
                        response.status = StatusCodes.Status409Conflict;
                        response.message = $"Nhân viên {entity.EmployeeCode} chưa được khóa kỳ dữ liệu lương!!!";
                        if (isTrans) await _dbContext.Database.RollbackTransactionAsync();
                        return response;
                    }    
                    data.IsLocked = false;
                    data.DateTracking = dateTimeNow;
                    data.UpdateDate = dateTimeNow;
                    data.UserSign2 = userId;

                    _dbContext.Payrolls.Attach(data);
                    _dbContext.Entry(data).State = EntityState.Modified;
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
        #endregion Command
    }
}
