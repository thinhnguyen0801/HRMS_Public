using Azure.Core;
using HNOne.DailyJob.Repositories;
using Quartz;

namespace HNOne.DailyJob
{
    public class DailyJob : IJob
    {
        private readonly IServiceProvider _provider;
        private readonly IDailyJobRepository _dailyJobRepository;
        private readonly ILogger<DailyJob> _logger;
        public DailyJob(IServiceProvider provider, ILogger<DailyJob> logger, IDailyJobRepository dailyJobRepository)
        {
            _provider = provider;
            _logger = logger;
            _dailyJobRepository = dailyJobRepository;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var lstJob = await _dailyJobRepository.GetDailyJob();
                if (lstJob == null || !lstJob.Any()) return;
                foreach (var job in lstJob)
                {
                    await _dailyJobRepository.UpdateDailyJob(job);
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Execute: {ex.Message}");
            }
        }
    }
}
