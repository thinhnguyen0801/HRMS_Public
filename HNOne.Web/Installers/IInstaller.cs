namespace HNOne.Web.Installers
{
    public interface IInstaller
    {
        void InstallerService(IServiceCollection services, IConfiguration configuration);
    }
}
