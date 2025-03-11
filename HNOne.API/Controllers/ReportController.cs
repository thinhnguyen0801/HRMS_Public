using HNOne.API.Repositories;
using HNOne.API.Repositories.Interfaces;
using HNOne.Common;
using HNOne.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace HNOne.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly ILogger<ReportController> _logger;
        private readonly IReportRepository _reportRepository;

        public ReportController(ILogger<ReportController> logger, IReportRepository reportRepository)
        {
            _logger = logger;
            _reportRepository = reportRepository;
        }

        [HttpPost]
        [Route("get-data")]
        public async Task<IActionResult> GetData([FromBody] RequestModel request)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                ConcurrentDictionary<string, object> data = new ConcurrentDictionary<string, object>();
                string? processKey = request.process?.Trim();
                switch (processKey)
                {
                    case ProcessConstants.GET_RPT_PAYROLL_SUMMARY:
                        response.data = await _reportRepository.GetRptPayrollSummary(request);
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
