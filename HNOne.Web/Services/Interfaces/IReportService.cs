using HNOne.Model;

namespace HNOne.Web.Services.Interfaces
{
    public interface IReportService
    {
        Task<List<T>?> GetMasterDataAsync<T>(RequestModel request, bool isShowToast = false) where T : class;
    }
}
