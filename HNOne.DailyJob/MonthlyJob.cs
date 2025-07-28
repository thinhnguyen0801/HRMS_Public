using HNOne.DailyJob.Repositories;
using Quartz;

namespace HNOne.DailyJob
{
    public class MonthlyJob : IJob
    {
        private readonly IServiceProvider _provider;
        private readonly IDailyJobRepository _dailyJobRepository;
        private readonly ILogger<MonthlyJob> _logger;
        public MonthlyJob(IServiceProvider provider, ILogger<MonthlyJob> logger, IDailyJobRepository dailyJobRepository)
        {
            _provider = provider;
            _logger = logger;
            _dailyJobRepository = dailyJobRepository;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                await _dailyJobRepository.CalculateALInfo();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Execute: {ex.Message}");
            }
        }
    }
}
