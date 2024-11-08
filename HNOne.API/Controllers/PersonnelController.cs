using HNOne.API.Services.Interfaces;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace HNOne.API.Controllers
{
    /// <summary>
    /// page danh cho Module nhân sự
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PersonnelController : ControllerBase
    {
        private readonly ILogger<PersonnelController> _logger;
        private readonly IPersonnelService _personnelService;
        public PersonnelController(IPersonnelService personnelService, ILogger<PersonnelController> logger)
        {
            _personnelService = personnelService;
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
                    case ProcessConstants.GET_EMPLOYEE:
                        response.data = await _personnelService.GetEmployee(request);
                        break;
                    case ProcessConstants.GET_CONTRACT:
                        response.data = await _personnelService.GetContract(request);
                        break;
                    case ProcessConstants.GET_FAMILYRELATIONSHIP:
                        response.data = await _personnelService.GetFamilyRelationship(request.employeeId);
                        break;
                    case ProcessConstants.GET_INSURANCE:
                        response.data = await _personnelService.GetInsurance(request.employeeId);
                        break;
                    case ProcessConstants.GET_CONTRACT_APPENDIX:
                        response.data = await _personnelService.GetContractAppendix(request);
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
                switch (processKey)
                {
                    case ProcessConstants.POST_EMPLOYEE:
                    case ProcessConstants.PUT_EMPLOYEE:
                        var employee = JsonConvert.DeserializeObject<Employees>($"{request.json}");
                        response = await _personnelService.UpdateEmployee(processKey, employee!);
                        break;
                    case ProcessConstants.POST_CONTRACT:
                    case ProcessConstants.PUT_CONTRACT:
                        var contract = JsonConvert.DeserializeObject<Contracts>($"{request.json}");
                        var lstSalaryConfig = JsonConvert.DeserializeObject<List<SalaryAdjustments>>($"{request.jsonDetail}");
                        response = await _personnelService.UpdateContract(processKey, contract!, lstSalaryConfig);
                        break;
                    case ProcessConstants.POST_FAMILYRELATIONSHIP:
                    case ProcessConstants.PUT_FAMILYRELATIONSHIP:
                        var familyRelationship = JsonConvert.DeserializeObject<FamilyRelationships>($"{request.json}");
                        response = await _personnelService.UpdateFamilyRelationship(processKey, familyRelationship!);
                        break;
                    case ProcessConstants.POST_INSURANCE:
                    case ProcessConstants.PUT_INSURANCE:
                        var insurance = JsonConvert.DeserializeObject<Insurances>($"{request.json}");
                        response = await _personnelService.UpdateInsurance(processKey, insurance!);
                        break;
                    case ProcessConstants.POST_CONTRACT_APPENDIX:
                    case ProcessConstants.PUT_CONTRACT_APPENDIX:
                        var contractAppendix = JsonConvert.DeserializeObject<ContractAppendices>($"{request.json}");
                        List<SalaryAdjustments>? lstSalaryAppendix = null;
                        if(!string.IsNullOrEmpty(request.jsonDetail)) lstSalaryAppendix = JsonConvert.DeserializeObject<List<SalaryAdjustments>>($"{request.jsonDetail}");
                        response = await _personnelService.UpdateContractAppendix(processKey, contractAppendix!, lstSalaryAppendix);
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
