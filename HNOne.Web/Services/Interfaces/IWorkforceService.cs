using HNOne.Model;
using HNOne.Model.Models;

namespace HNOne.Web.Services.Interfaces
{
    public interface IWorkforceService
    {
        Task<List<T>?> GetMasterDataAsync<T>(RequestModel request, bool isShowToast = false) where T : class;
        Task<bool> UpdateMasterDataAsync(RequestModel request);
        Task<int> UpdateLeaveRequestAsync(string processKey, int userId, string token, int branchId, string json, string jsonDetail);
        Task<List<LeaveRequestModel>?> GetLeaveRequestAsync(RequestModel request, bool isShowToast = false);
        Task<List<ShiftChangeModel>?> GetShiftChangeRequestAsync(RequestModel request, bool isShowToast = false);
        Task<List<OvertimeRequestModel>?> GetOvertimeRequestAsync(RequestModel request, bool isShowToast = false);
    }
}
