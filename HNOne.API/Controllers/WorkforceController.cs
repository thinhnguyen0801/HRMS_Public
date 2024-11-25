using Azure;
using HNOne.API.Repositories.Interfaces;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Entities;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace HNOne.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkforceController : ControllerBase
    {
        private readonly ILogger<WorkforceController> _logger;
        private readonly IWorkforceRepository _workforceRepository;

        public WorkforceController(IWorkforceRepository workforceRepository, ILogger<WorkforceController> logger)
        {
            _workforceRepository = workforceRepository;
            _logger = logger;
        }

        /// <summary>
        /// lấy dữ liệu
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        //[Authorize] // sau có token mở ra
        [HttpPost]
        [Route("get-data")]
        public async Task<IActionResult> GetData([FromBody] RequestModel request)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                string? processKey = request.process?.Trim();
                switch (processKey)
                {
                    case ProcessConstants.GET_LEAVE_CONFIG:
                        response.data = await _workforceRepository.GetLeaveConfig(request);
                        break;
                    case ProcessConstants.GET_WORKFORCE_MASTER_DATA:
                        response.data = await _workforceRepository.GetWorkforceMasterData(request);
                        break;
                    case ProcessConstants.GET_LEAVE_REQUEST:
                        response.data = await _workforceRepository.GetLeaveRequest(request);
                        break;
                    case ProcessConstants.GET_LEAVE_WORKING_HOUR:
                        response.data = await _workforceRepository.GetLeaveWorkingHour(request);
                        break;
                    case ProcessConstants.GET_HOILDAY_CATAGORY:
                        response.data = await _workforceRepository.GetHolidayCatagory(request);
                        break;
                    case ProcessConstants.GET_SHIFT_CHANGE_REQUEST:
                        response.data = await _workforceRepository.GetShiftChange(request);
                        break;
                    case ProcessConstants.GET_OVERTIME_REQUEST:
                        response.data = await _workforceRepository.GetOvertimeRequest(request);
                        break;
                    case ProcessConstants.GET_WORK_CONFIG:
                        response.data = await _workforceRepository.GetWorkConfig(request);
                        break;
                    case ProcessConstants.GET_ARRANGE_SHIFT:
                        response.data = await _workforceRepository.GetArrangeShift(request);
                        break;
                    case ProcessConstants.GET_WORK_SHEDULE:
                        response.data = await _workforceRepository.GetWorkShedule(request);
                        break;
                    case ProcessConstants.GET_ANNUAL_LEAVE_INFO:
                        response.data = await _workforceRepository.GetAnnualLeaveInfo(request);
                        break;
                    case ProcessConstants.GET_CHECK_IN_OUT:
                        response.data = await _workforceRepository.GetCheckInOut(request);
                        break;
                    default:
                        response.status = StatusCodes.Status404NotFound;
                        response.message = $"Process Key {processKey} was not provider!!!";
                        return Ok(response);
                }    
                if ((response.data is IEnumerable<object> dataList) && dataList.IsNullOrEmpty())
                {
                    response.status = StatusCodes.Status204NoContent;
                    response.message = "Không tìm thấy dữ liệu!!!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request.");
                response.status = StatusCodes.Status400BadRequest;
                response.message = ex.Message;
            }
            return Ok(response);
        }

        [HttpPost]
        [Route("post-data")]
        public async Task<IActionResult> PostData([FromBody] RequestModel request)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                string? processKey = request.process?.Trim();
                // Helper function for deserialization to reduce duplication
                T DeserializeJson<T>(string json) => JsonConvert.DeserializeObject<T>(json)!;
                switch (processKey)
                {
                    case ProcessConstants.POST_LEAVE_CONFIG:
                    case ProcessConstants.PUT_LEAVE_CONFIG:
                        var leaveConfig = DeserializeJson<LeaveConfigs>($"{request.json}");
                        response = await _workforceRepository.UpdateLeaveConfig(processKey, leaveConfig!);
                        break;
                    case ProcessConstants.POST_LEAVE_REQUEST:
                        var leaveRequestPost = DeserializeJson<LeaveRequests>($"{request.json}");
                        var lstRequest1Post = DeserializeJson<List<LeaveRequest1s>>($"{request.jsonDetail}");
                        response = await _workforceRepository.AddLeaveRequest(leaveRequestPost!, lstRequest1Post!);
                        break;
                    case ProcessConstants.PUT_LEAVE_REQUEST:
                        var leaveRequestPut = DeserializeJson<LeaveRequests>($"{request.json}");
                        var lstRequest1Put = DeserializeJson<List<LeaveRequest1s>>($"{request.jsonDetail}");
                        response = await _workforceRepository.UpdateLeaveRequest(leaveRequestPut!, lstRequest1Put!);
                        break;
                    case ProcessConstants.POST_LEAVE_WORKING_HOUR:
                    case ProcessConstants.PUT_LEAVE_WORKING_HOUR:
                        var leaveWorkingHour = DeserializeJson<LeaveWorkingHours>($"{request.json}");
                        response = await _workforceRepository.UpdateLeaveWorkingHours(processKey, leaveWorkingHour!);
                        break;
                    case ProcessConstants.POST_HOILDAY_CATAGORY:
                    case ProcessConstants.PUT_HOILDAY_CATAGORY:
                        var holiday = DeserializeJson<HolidayCatagories>($"{request.json}");
                        response = await _workforceRepository.UpdateHolidayCatagory(processKey, holiday!);
                        break;
                    case ProcessConstants.POST_SHIFT_CHANGE_REQUEST:
                        var shiftChangePost = DeserializeJson<ShiftChanges>($"{request.json}");
                        var lstShiftChangePost = DeserializeJson<List<ShiftChange1s>>($"{request.jsonDetail}");
                        response = await _workforceRepository.AddShiftChangeRequest(shiftChangePost!, lstShiftChangePost!);
                        break;
                    case ProcessConstants.PUT_SHIFT_CHANGE_REQUEST:
                        var shiftChangePut = DeserializeJson<ShiftChanges>($"{request.json}");
                        var lstShiftChangePut = DeserializeJson<List<ShiftChange1s>>($"{request.jsonDetail}");
                        response = await _workforceRepository.UpdateShiftChangeRequest(shiftChangePut!, lstShiftChangePut!);
                        break;
                    case ProcessConstants.POST_OVERTIME_REQUEST:
                        var overtimeRequestPost = DeserializeJson<OvertimeRequests>($"{request.json}");
                        var lstovertimeRequestPost = DeserializeJson<List<OvertimeRequest1s>>($"{request.jsonDetail}");
                        response = await _workforceRepository.AddOvertimeRequest(overtimeRequestPost!, lstovertimeRequestPost!);
                        break;
                    case ProcessConstants.PUT_OVERTIME_REQUEST:
                        var overtimeRequestPut = DeserializeJson<OvertimeRequests>($"{request.json}");
                        var lstovertimeRequestPut = DeserializeJson<List<OvertimeRequest1s>>($"{request.jsonDetail}");
                        response = await _workforceRepository.UpdateOvertimeRequest(overtimeRequestPut!, lstovertimeRequestPut!);
                        break;
                    case ProcessConstants.POST_WORK_CONFIG:
                        response = await _workforceRepository.GenerateWorkConfig(request);
                        break;
                    case ProcessConstants.POST_TIME_SHEET:
                        response = await _workforceRepository.GenerateTimesheets(request);
                        break;
                    case ProcessConstants.POST_SHIFT_ASSIGNMENT:
                        var lstShiftAssignmentPut = DeserializeJson<List<ShiftAssignments>>($"{request.json}");
                        response = await _workforceRepository.UpdateShiftAssignment(lstShiftAssignmentPut!);
                        break;
                    default:
                        response.status = StatusCodes.Status404NotFound;
                        response.message = $"Process Key {processKey} was not provider!!!";
                        return Ok(response);
                }    
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request.");
                response.status = StatusCodes.Status400BadRequest;
                response.message = ex.Message;
            }
            return Ok(response);
        }
    }
}
