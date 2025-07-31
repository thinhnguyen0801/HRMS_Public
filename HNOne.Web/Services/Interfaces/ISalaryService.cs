using HNOne.Model;
using HNOne.Model.Models;

namespace HNOne.Web.Services.Interfaces
{
    public interface ISalaryService
    {
        Task<List<T>?> GetMasterDataAsync<T>(RequestModel request, bool isShowToast = false) where T : class;
        Task<bool> UpdateMasterDataAsync(RequestModel request);
        Task<List<PayrollModel>?> SalaryCalculateAsync(RequestModel request);
        Task<string> UpdatePayrollAsync(RequestModel request);
        Task<int> UpdateDocumentAsync(string processKey, int userId, string token, int branchId, string json, string jsonDetail, bool isShowToast = true);
    }
}
