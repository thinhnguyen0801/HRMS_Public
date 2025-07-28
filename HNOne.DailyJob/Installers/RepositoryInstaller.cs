using HNOne.DailyJob.Repositories;
using Quartz;

namespace HNOne.DailyJob.Installers
{
    public class RepositoryInstaller : IInstaller
    {
        private const string CONN_NAME = "DbConnection";
        public void InstallerService(IServiceCollection services, IConfiguration configuration)
        {
            string timeJobCron = configuration.GetSection("QuartzSettings:TimeJobCron").Value ?? "0 0 1 * * ?"; // chạy 1h sáng mỗi ngày
            string timeJobCronMonthly = configuration.GetSection("QuartzSettings:TimeJobCronMonthly").Value ?? "0 0 1 * * ?"; // chạy 1h sáng mỗi ngày
            services.AddTransient<IDapperDbContext, DapperDbContext>();
            services.AddScoped<IDailyJobRepository, DailyJobRepository>();
            services.AddQuartz(options =>
            {
                var jobKey = new JobKey("JobUpdate");
                options.AddJob<DailyJob>(opts => opts.WithIdentity(jobKey));
                options.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity("JobUpdate-trigger")
                .WithCronSchedule(timeJobCron));

                // job tính toán thông tin phép năm
                var jobCalcALInfo = new JobKey("JobCalcALInfo");
                options.AddJob<MonthlyJob>(opts => opts.WithIdentity(jobCalcALInfo));
                options.AddTrigger(opts => opts
                .ForJob(jobCalcALInfo)
                .WithIdentity("JobCalcALInfo-trigger")
                .WithCronSchedule(timeJobCronMonthly));
            });
            services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
        }


    }
}
