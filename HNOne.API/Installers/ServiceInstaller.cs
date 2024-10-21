
using HNOne.API.Services;
using HNOne.API.Services.Interfaces;

namespace HNOne.API.Installers
{
    public class ServiceInstaller : IInstaller
    {
        public void InstallerService(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IMasterDataService, MasterDataService>();
            services.AddScoped<IUserService, UserService>();
        }
    }
}
