

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
    }
}
