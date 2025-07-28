using Azure.Core;
using Azure;
using HNOne.DailyJob.Repositories;
using HNOne.Model;
using Microsoft.AspNetCore.Mvc;

namespace HNOne.DailyJob.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DailyJobController : ControllerBase
    {
        private readonly IDailyJobRepository _dailyJobRepository;
        private readonly ILogger<DailyJobController> _logger;

        public DailyJobController(ILogger<DailyJobController> logger, IDailyJobRepository dailyJobRepository)
        {
            _logger = logger;
            _dailyJobRepository = dailyJobRepository;
        }


        /// <summary>
        /// Thực thi trigger
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ExecuteJob()
        {
            ResponseModel response = new ResponseModel();
            try
            {
                var lstJob = await _dailyJobRepository.GetDailyJob();
                if (lstJob != null && lstJob.Any())
                {
                    foreach (var job in lstJob)
                    {
                        await _dailyJobRepository.UpdateDailyJob(job);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"PostData: {ex.Message}");
                response.status = StatusCodes.Status400BadRequest;
                response.message = ex.Message;
            }
            return Ok(response);
        }
    }
}
