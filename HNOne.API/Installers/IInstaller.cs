namespace HNOne.API.Installers
{
    public interface IInstaller
    {
        void InstallerService(IServiceCollection services, IConfiguration configuration);
    }
}
