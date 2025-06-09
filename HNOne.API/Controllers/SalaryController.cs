using HNOne.API.Repositories;
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
    public class SalaryController : ControllerBase
    {
        private readonly ILogger<SalaryController> _logger;
        private readonly ISalaryRepository _salaryRepository;
        public SalaryController(ILogger<SalaryController> logger, ISalaryRepository salaryRepository)
        {
            _logger = logger;
            _salaryRepository = salaryRepository;
        }

        [HttpPost]
        [Route("get-data")]
        public async Task<IActionResult> GetData([FromBody] RequestModel request)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                string? processKey = request.process?.Trim();
                switch(processKey)
                {
                    case ProcessConstants.GET_MONTHLY_SALARY:
                        response.data = await _salaryRepository.GetMonthlySalary(request);
                        break;
                    case ProcessConstants.GET_PAYROLL_SUMMARY:
                        response.data = await _salaryRepository.GetPayrollSummary(request);
                        break;
                    case ProcessConstants.GET_SALARY_MASTER_DATA:
                        response.data = await _salaryRepository.GetSalaryMasterData(request);
                        break;
                    case ProcessConstants.GET_SALARY_EXPENSE_ACCOUNTING:
                        response.data = await _salaryRepository.GetSalaryExpenseAccounting(request);
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
                _logger.LogError(ex, $"GetData: {ex.Message}. Request: {JsonConvert.SerializeObject(request)}");
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
                    case ProcessConstants.POST_PAYROLL_SALARY:
                        var lstPayroll = DeserializeJson<List<Payrolls>>($"{request.json}");
                        string typeLocked = "N";
                        if(!string.IsNullOrEmpty(request.type)) typeLocked = request.type;
                        response = await _salaryRepository.UpdatePayroll(request.branchId, request.userId, processKey, typeLocked, lstPayroll);
                        //response = await _salaryRepository.UpdatePayroll(request.type == "L", request.userId, lstPayroll!);
                        break;
                    case ProcessConstants.PUT_UNLOCK_PAYROLL_SALARY:
                        var lstPayrollUnlock = DeserializeJson<List<Payrolls>>($"{request.json}");
                        response = await _salaryRepository.UnLockPayroll(request.userId, lstPayrollUnlock!);
                        break;
                    case ProcessConstants.POST_SALARY_EXPENSE_ACCOUNTING:
                    case ProcessConstants.PUT_SALARY_EXPENSE_ACCOUNTING:
                        var salaryExpenseAccounting = DeserializeJson<SalaryExpenseAccountings>($"{request.json}");
                        var lstsalaryExpenseAccounting1s = DeserializeJson<List<SalaryExpenseAccounting1s>>($"{request.jsonDetail}");
                        response = await _salaryRepository.UpdateSalaryExpenseAccounting(processKey, salaryExpenseAccounting!, lstsalaryExpenseAccounting1s!);
                        break;
                    default:
                        response.status = StatusCodes.Status404NotFound;
                        response.message = $"Process Key {processKey} was not provider!!!";
                        return Ok(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"PostData: {ex.Message}. Request: {JsonConvert.SerializeObject(request)}");
                response.status = StatusCodes.Status400BadRequest;
                response.message = ex.Message;
            }
            return Ok(response);
        }
    }
}
