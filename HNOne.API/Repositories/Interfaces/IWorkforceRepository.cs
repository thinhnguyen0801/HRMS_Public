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
        Task<ResponseModel> UpdateHolidayCatagory(string actionType, HolidayCatagories entity);
        Task<IEnumerable<HolidayCatagoryModel>> GetHolidayCatagory(RequestModel request);
        Task<IEnumerable<ShiftChangeModel>> GetShiftChange(RequestModel request);
        Task<ResponseModel> AddShiftChangeRequest(ShiftChanges entity, IEnumerable<ShiftChange1s> lstEntity1);
        Task<ResponseModel> UpdateShiftChangeRequest(ShiftChanges entity, IEnumerable<ShiftChange1s> lstEntity1);
        Task<IEnumerable<OvertimeRequestModel>> GetOvertimeRequest(RequestModel request);
        Task<ResponseModel> AddOvertimeRequest(OvertimeRequests entity, IEnumerable<OvertimeRequest1s> lstEntity1);
        Task<ResponseModel> UpdateOvertimeRequest(OvertimeRequests entity, IEnumerable<OvertimeRequest1s> lstEntity1);
        Task<IEnumerable<WorkConfigModel>> GetWorkConfig(RequestModel request);
        Task<ResponseModel> GenerateWorkConfig(RequestModel request);
        Task<IEnumerable<ShiftAssignmentModel>> GetArrangeShift(RequestModel request);
        Task<IEnumerable<TimesheetModel>> GetWorkShedule(RequestModel request);
        Task<IEnumerable<AnnualLeaveInfoModel>> GetAnnualLeaveInfo(RequestModel request);
    }
}
