

using HNOne.Model.Models;
using HNOne.Model;
using HNOne.Model.Entities;

namespace HNOne.API.Repositories.Interfaces
{
    public interface ISalaryRepository
    {
        Task<ResponseModel> UpdatePayroll(bool isLocked, int userId, IEnumerable<Payrolls> lstEntity);
        Task<IEnumerable<PayrollModel>> GetMonthlySalary(RequestModel request);
        Task<ResponseModel> UnLockPayroll(int userId, IEnumerable<Payrolls> lstEntity);
        Task<IEnumerable<PayrollModel>> GetPayrollSummary(RequestModel request);
        Task<ResponseModel> UpdatePayroll(int branchId, int userId, string processKey, string typeLocked, IEnumerable<Payrolls> lstEntity);
        Task<IEnumerable<dynamic>> GetSalaryMasterData(RequestModel request);
        Task<IEnumerable<SalaryExpenseAccountingModel>> GetSalaryExpenseAccounting(RequestModel request);
        Task<ResponseModel> UpdateSalaryExpenseAccounting(string actionType, SalaryExpenseAccountings entity, IEnumerable<SalaryExpenseAccounting1s> lstEntity1);
    }
}
