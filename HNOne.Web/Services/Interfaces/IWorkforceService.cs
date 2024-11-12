using HNOne.Model;

namespace HNOne.Web.Services.Interfaces
{
    public interface IWorkforceService
    {
        Task<List<T>?> GetMasterDataAsync<T>(RequestModel request, bool isShowToast = false) where T : class;
        Task<bool> UpdateMasterDataAsync(RequestModel request);
    }
}
