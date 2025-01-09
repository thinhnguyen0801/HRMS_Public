using HNOne.Model;

namespace HNOne.Web.Services.Interfaces
{
    public interface ISalaryService
    {
        Task<List<T>?> GetMasterDataAsync<T>(RequestModel request, bool isShowToast = false) where T : class;
        Task<bool> UpdateMasterDataAsync(RequestModel request);
    }
}
