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
