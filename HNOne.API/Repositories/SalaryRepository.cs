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

        /// <summary>
        /// lấy danh sách dữ liệu bảng lương đã lưu
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IEnumerable<PayrollModel>> GetPayrollSummary(RequestModel request)
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
                parameters.Add("@StatusIds", $"{request.opt2}");
                parameters.Add("@EmployeeIds", $"{request.opt3}");
                parameters.Add("@Type", $"{request.type}");
                var lstResult = await connection.QueryAsync<PayrollModel>(StoreConstants.STORE_H1_PAYROLL_SUMMARY_SELECT, param: parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.StoredProcedure);
                return lstResult;
            }
        }

        /// <summary>
        /// lấy ra danh sách master dưới store trong phân hệ lương
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IEnumerable<dynamic>> GetSalaryMasterData(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", request.userId, DbType.Int32);
                parameters.Add("@BranchId", request.branchId, DbType.Int32);
                parameters.Add("@EmployeeId", request.employeeId, DbType.Int32);
                parameters.Add("@Type", request.type, DbType.String);
                parameters.Add("@Opt", $"{request.opt}", DbType.String);
                parameters.Add("@Opt1", $"{request.opt1}", DbType.String);
                parameters.Add("@Opt2", $"{request.opt2}", DbType.String);
                parameters.Add("@Opt3", $"{request.opt3}", DbType.String);
                var results = await connection.QueryAsync(StoreConstants.STORE_H1_SALARY_MASTER_DATA_SELECT, parameters
                    , commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.StoredProcedure);
                return results;
            }
        }

        public async Task<IEnumerable<SalaryExpenseAccountingModel>> GetSalaryExpenseAccounting(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                request.fromDate ??= new DateTime(2000, 01, 01);
                request.toDate ??= DateTime.Now.AddMonths(1);
                var parameters = new DynamicParameters();
                parameters.Add("@DocumentId", request.documentId, DbType.Int32);
                parameters.Add("@UserId", request.userId, DbType.Int32);
                parameters.Add("@BranchId", request.branchId, DbType.Int32);
                parameters.Add("@StatusIds", request.opt, DbType.String);
                parameters.Add("@FromDate", request.fromDate, DbType.Date);
                parameters.Add("@ToDate", request.toDate, DbType.Date);
                IEnumerable<SalaryExpenseAccountingModel>? lstResult = null;
                var dtResult = await connection.QueryMultipleAsync(StoreConstants.STORE_H1_SALARY_EXPENSE_ACCOUNTING_SELECT, param: parameters
                    , commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.StoredProcedure);
                if (dtResult != null)
                {
                    lstResult = dtResult.Read<SalaryExpenseAccountingModel>();
                    if (request.documentId > 0)
                    {
                        var lstDetail = dtResult.Read<SalaryExpenseAccounting1Model>();
                        string jsonDetail = JsonConvert.SerializeObject(lstDetail);
                        lstResult = lstResult.Update(m => m.jsonDetail = jsonDetail);
                    }
                }
                return lstResult ?? new List<SalaryExpenseAccountingModel>();
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
                    #region Dữ liệu công của nhân viên
                    data.BranchId = entity.BranchId;
                    data.EmployeeCode = entity.EmployeeCode;
                    data.EmployeeName = entity.EmployeeName;
                    data.DepartmentId = entity.DepartmentId;
                    data.DepartmentCode = entity.DepartmentCode;
                    data.DepartmentName = entity.DepartmentName;
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
                    data.IsCompanyIncomeTax = entity.IsCompanyIncomeTax;
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
                    data.DeductionConfigSIId = entity.DeductionConfigSIId;
                    data.CoefficientEnterpriseSI = entity.CoefficientEnterpriseSI;
                    data.CoefficientEmployeeSI = entity.CoefficientEmployeeSI;
                    data.ContributionSalarySI = entity.ContributionSalarySI;
                    data.DeductionEnterpriseSI = entity.DeductionEnterpriseSI;
                    data.DeductionEmployeeSI = entity.DeductionEmployeeSI;

                    data.DeductionConfigHIId = entity.DeductionConfigHIId;
                    data.CoefficientEnterpriseHI = entity.CoefficientEnterpriseHI;
                    data.CoefficientEmployeeHI = entity.CoefficientEmployeeHI;
                    data.ContributionSalaryHI = entity.ContributionSalaryHI;
                    data.DeductionEnterpriseHI = entity.DeductionEnterpriseHI;
                    data.DeductionEmployeeHI = entity.DeductionEmployeeHI;

                    data.DeductionConfigUIId = entity.DeductionConfigUIId;
                    data.CoefficientEnterpriseUI = entity.CoefficientEnterpriseUI;
                    data.CoefficientEmployeeUI = entity.CoefficientEmployeeUI;
                    data.ContributionSalaryUI = entity.ContributionSalaryUI;
                    data.DeductionEnterpriseUI = entity.DeductionEnterpriseUI;
                    data.DeductionEmployeeUI = entity.DeductionEmployeeUI;

                    data.DeductionConfigAIId = entity.DeductionConfigAIId;
                    data.CoefficientEnterpriseAI = entity.CoefficientEnterpriseAI;
                    data.CoefficientEmployeeAI = entity.CoefficientEmployeeAI;
                    data.ContributionSalaryAI = entity.ContributionSalaryAI;
                    data.DeductionEnterpriseAI = entity.DeductionEnterpriseAI;
                    data.DeductionEmployeeAI = entity.DeductionEmployeeAI;
                    data.TotalDeductionEnterprise = entity.TotalDeductionEnterprise;
                    data.TotalDeductionEmployee = entity.TotalDeductionEmployee;
                    data.TotalDeduction = entity.TotalDeduction;

                    data.DeductionConfigUFId = entity.DeductionConfigUFId;
                    data.CoefficientEnterpriseUF = entity.CoefficientEnterpriseUF;
                    data.CoefficientEmployeeUF = entity.CoefficientEmployeeUF;
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
        /// cập nhật dữ liệu payroll
        /// </summary>
        /// <param name="branchId"></param>
        /// <param name="userId"></param>
        /// <param name="processKey"></param>
        /// <param name="lstEntity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdatePayroll(int branchId, int userId, string processKey, string typeLocked, IEnumerable<Payrolls> lstEntity)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                ResponseModel response = new ResponseModel();
                if (lstEntity.IsNullOrEmpty())
                {
                    response.status = StatusCodes.Status409Conflict;
                    response.message = MessageConstants.MESSAGE_DATA_INVALID;
                    return response;
                }    
                string jPayroll = JsonConvert.SerializeObject(lstEntity);
                var parameters = new DynamicParameters();
                parameters.Add($"@UserId", userId);
                parameters.Add($"@BranchId", branchId);
                parameters.Add($"@JPayroll", jPayroll);
                parameters.Add($"@ProcessKey", processKey);
                parameters.Add($"@TypeLocked", typeLocked);
                var result = await connection.QueryFirstOrDefaultAsync<ResponseModel>(StoreConstants.STORE_H1_PAYROLL_SUMMARY_UPDATE
                    , param: parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.StoredProcedure);
                if(result != null)
                {
                    response.status = result.status;
                    response.message = result.message;
                }    
                return response;
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

        /// <summary>
        /// Thêm mới + cập nhật dữ liệu hạch toán chi phí lương
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="entity"></param>
        /// <param name="lstEntity1"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateSalaryExpenseAccounting(string actionType, SalaryExpenseAccountings entity, IEnumerable<SalaryExpenseAccounting1s> lstEntity1)
        {
            bool isTrans = false;
            ResponseModel response = new ResponseModel();
            try
            {
                DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();  
                if (actionType == ProcessConstants.POST_SALARY_EXPENSE_ACCOUNTING)
                {
                    using (var connection = _dapperDbContext.CreateConnection())
                    {
                        // Kiểm tra tính lương có ông nào chưa được khóa không
                        var checkLocked = await _dbContext.Payrolls.FirstOrDefaultAsync(m => m.IsLocked == false && m.IsDelete == false
                                            && m.Month == entity.Month && m.Year == entity.Year);
                        if (checkLocked != null)
                        {
                            response.status = StatusCodes.Status409Conflict;
                            response.message = $"Nhân viên {checkLocked.EmployeeCode} chưa được khóa kỳ dữ liệu lương tháng {checkLocked.Month} năm {checkLocked.Year}";
                            return response;
                        }

                        // kiểm tra dữ liệu hạch toán đã tồn tại chưa
                        var checkExists = await _dbContext.SalaryExpenseAccountings.FirstOrDefaultAsync(m => m.Month == entity.Month && m.Year == entity.Year
                                && m.IsDelete == false && (m.StatusCode != CommonConstants.STATUS_CODE_CANCELED && m.StatusCode != CommonConstants.STATUS_CODE_DENY));
                        if (checkExists != null)
                        {
                            response.status = StatusCodes.Status409Conflict;
                            response.message = $"Hạch toán chi phí lương tháng {checkExists.Month} năm {checkExists.Year} đã tồn tại trong hệ thống. Số chứng từ [{checkExists.VoucherNo}]";
                            return response;
                        }

                        DynamicParameters parameters = new DynamicParameters();
                        parameters.Add("@Type", GlobalConstants.TABLE_SALARY_EXPENSE_ACCOUNTING, DbType.String);
                        string commandText = @$"select {StoreConstants.FUNC_GET_VOUCHER}(@Type, '', '', '')";
                        string? voucherNo = await connection.QueryFirstOrDefaultAsync<string>(commandText, param: parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                        if (string.IsNullOrEmpty(voucherNo))
                        {
                            response.status = StatusCodes.Status204NoContent;
                            response.message = MessageConstants.MESSAGE_VOUCHER_NO_MISSING;
                            return response;
                        }
                        await _dbContext.Database.BeginTransactionAsync();
                        isTrans = true;
                        entity.Id = await _dbContext.SalaryExpenseAccountings.Select(m => m.Id).DefaultIfEmpty().MaxAsync() + 1;
                        entity.VoucherNo = voucherNo;
                        entity.DateTracking = dateTimeNow;
                        entity.CreateDate = dateTimeNow;
                        await _dbContext.SalaryExpenseAccountings.AddAsync(entity);
                        // thêm chi tiết
                        foreach (var item in lstEntity1)
                        {
                            SalaryExpenseAccounting1s entity1 = new SalaryExpenseAccounting1s();
                            entity1.SalaryExpenseAccountingId = entity.Id;
                            entity1.LineId = item.LineId;
                            entity1.SalaryCatagoryCode = item.SalaryCatagoryCode;
                            entity1.SalaryCatagoryName = item.SalaryCatagoryName;
                            entity1.Account1 = item.Account1;
                            entity1.Account2 = item.Account2;
                            entity1.LineTotal = item.LineTotal;
                            entity1.DateTracking = dateTimeNow;
                            entity1.UserSign = entity.UserSign;
                            await _dbContext.SalaryExpenseAccounting1s.AddAsync(entity1);
                        }
                        await _dbContext.SaveChangesAsync();
                        await _dbContext.Database.CommitTransactionAsync();
                        response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                        response.data = entity.Id;
                    }
                    
                }    
                else
                {
                    var data = await _dbContext.SalaryExpenseAccountings.FirstOrDefaultAsync(m => m.Id == entity.Id);
                    if (data == null)
                    {
                        response.status = StatusCodes.Status404NotFound;
                        response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                        return response;
                    }
                    if (data.DateTracking != entity.DateTracking)
                    {
                        response.status = StatusCodes.Status409Conflict;
                        response.message = MessageConstants.MESSAGE_DATA_CHECKING_MODIFIED;
                        return response;
                    }
                    data.EmployeeSignatureId = entity.EmployeeSignatureId;
                    data.Month = entity.Month;
                    data.Year = entity.Year;
                    data.DueDate = entity.DueDate;
                    data.Remark = entity.Remark;
                    data.DocTotal = entity.DocTotal;
                    data.DateTracking = dateTimeNow;
                    data.UpdateDate = dateTimeNow;
                    data.UserSign2 = entity.UserSign2;
                    await _dbContext.Database.BeginTransactionAsync();
                    isTrans = true;
                    _dbContext.SalaryExpenseAccountings.Attach(data);
                    _dbContext.Entry(data).State = EntityState.Modified;
                    // thêm chi tiết đề nghị nghỉ phép
                    // bỏ dữ liệu củ đi
                    var lstLeaveRequest1s = await _dbContext.SalaryExpenseAccounting1s.Where(m => m.SalaryExpenseAccountingId == data.Id).ToListAsync();
                    if (!lstLeaveRequest1s.IsNullOrEmpty()) _dbContext.SalaryExpenseAccounting1s.RemoveRange(lstLeaveRequest1s);
                    // thêm chi tiết
                    foreach (var item in lstEntity1)
                    {
                        SalaryExpenseAccounting1s entity1 = new SalaryExpenseAccounting1s();
                        entity1.SalaryExpenseAccountingId = entity.Id;
                        entity1.LineId = item.LineId;
                        entity1.SalaryCatagoryCode = item.SalaryCatagoryCode;
                        entity1.SalaryCatagoryName = item.SalaryCatagoryName;
                        entity1.Account1 = item.Account1;
                        entity1.Account2 = item.Account2;
                        entity1.LineTotal = item.LineTotal;
                        entity1.DateTracking = dateTimeNow;
                        entity1.UserSign = entity.UserSign;
                        await _dbContext.SalaryExpenseAccounting1s.AddAsync(entity1);
                    }
                    await _dbContext.SaveChangesAsync();
                    await _dbContext.Database.CommitTransactionAsync();
                    response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
                    response.data = data.Id;
                }
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
