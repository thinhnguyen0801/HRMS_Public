using HNOne.Model.Entities;
using HNOne.Model;
using HNOne.Model.Models;

namespace HNOne.API.Repositories.Interfaces
{
    public interface IWorkforceRepository
    {
        Task<IEnumerable<LeaveConfigModel>> GetLeaveConfig(RequestModel request);
        Task<ResponseModel> UpdateLeaveConfig(string actionType, LeaveConfigs entity);
        Task<IEnumerable<dynamic>> GetWorkforceMasterData(RequestModel request);
        Task<ResponseModel> AddLeaveRequest(LeaveRequests entity, IEnumerable<LeaveRequest1s> lstEntity1);
        Task<ResponseModel> UpdateLeaveRequest(LeaveRequests entity, IEnumerable<LeaveRequest1s> lstEntity1);
        Task<IEnumerable<LeaveRequestModel>> GetLeaveRequest(RequestModel request);
        Task<IEnumerable<LeaveRequestModel>> GetLeaveWorkingHour(RequestModel request);
        Task<ResponseModel> UpdateLeaveWorkingHours(string actionType, LeaveWorkingHours entity);
    }
}
