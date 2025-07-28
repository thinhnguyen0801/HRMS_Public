namespace HNOne.DailyJob.Installers
{
    public interface IInstaller
    {
        void InstallerService(IServiceCollection services, IConfiguration configuration);
    }
}
