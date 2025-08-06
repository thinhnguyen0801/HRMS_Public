using Dapper;
using HNOne.API.Constants;
using HNOne.API.Repositories.Interfaces;
using HNOne.Model.Models;
using HNOne.Model;
using System.Data;

namespace HNOne.API.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly IDapperDbContext _dapperDbContext;

        public ReportRepository(IDapperDbContext dapperDbContext)
        {
            _dapperDbContext = dapperDbContext;
        }

        #region
        /// <summary>
        /// lấy danh sách dữ liệu báo cáo bảng lương
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IEnumerable<dynamic>> GetRptPayrollSummary(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", request.userId);
                parameters.Add("@BranchId", request.branchId);
                parameters.Add("@Year", request.year);
                parameters.Add("@Month", request.month);
                parameters.Add("@DepartmentIds", $"{request.departmentIds}");
                parameters.Add("@BranchIds", $"{request.branchIds}");
                parameters.Add("@StatusIds", $"{request.opt1}");
                parameters.Add("@EmployeeIds", $"{request.opt2}");
                parameters.Add("@Type", $"{request.type}");
                parameters.Add("@DocumentId",request.documentId);
                var lstResult = await connection.QueryAsync<dynamic>(StoreConstants.STORE_H1_RPT_PAYROLL_SUMMARY_SELECT, param: parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.StoredProcedure);
                return lstResult;
            }
        }
        
        /// <summary>
        /// Báo cáo tổng hợp trang dashboard
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IEnumerable<dynamic>> GetRptSummanry(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", request.userId);
                parameters.Add("@BranchId", request.branchId);
                parameters.Add("@Type", $"{request.type}");
                parameters.Add("@NumOfDay", $"{request.opt}");
                var lstResult = await connection.QueryAsync<dynamic>(StoreConstants.STORE_H1_RPT_SUMMARY_SELECT, param: parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.StoredProcedure);
                return lstResult;
            }
        }
        #endregion
    }
}
