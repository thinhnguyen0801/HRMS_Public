using HNOne.Model;

namespace HNOne.Web.Services.Interfaces
{
    public interface ITrainingService
    {
        Task<List<T>?> GetMasterDataAsync<T>(RequestModel request, bool isShowToast = false) where T : class;
        Task<int> UpdateTrainingAsync(string processKey, int userId, string token, int branchId, string json, string jsonDetail, bool isShowToast = true);
    }
}
