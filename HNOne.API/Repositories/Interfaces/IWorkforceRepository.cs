using HNOne.Model.Entities;
using HNOne.Model;
using HNOne.Model.Models;

namespace HNOne.API.Repositories.Interfaces
{
    public interface IWorkforceRepository
    {
        Task<IEnumerable<LeaveConfigModel>> GetLeaveConfig(RequestModel request);
        Task<ResponseModel> UpdateLeaveConfig(string actionType, LeaveConfigs entity);
    }
}
