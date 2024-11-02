using HNOne.Model;
using HNOne.Model.Models;

namespace HNOne.Web.Services.Interfaces
{
    public interface IPersonnelService
    {
        Task<List<EmployeeModel>?> GetEmployeeAsync(RequestModel request);
        Task<bool> UpdateEmployeeAsync(string processKey, int userId, string token, string json);
        Task<int> UpdateContractAsync(string processKey, int userId, string token, int branchId, string json, string jsonDetail);
        Task<List<ContractModel>?> GetContractAsync(RequestModel request, bool isShowToast = false);
    }
}
