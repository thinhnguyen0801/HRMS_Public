
using HNOne.API.Services;
using HNOne.API.Services.Interfaces;
using HNOne.Common;

namespace HNOne.API.Installers
{
    public class ServiceInstaller : IInstaller
    {
        public void InstallerService(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IDateTimeHelper, DateTimeHelper>();
            services.AddSingleton<IEncryptHelper, EncryptHelper>();
            services.AddScoped<IMasterDataService, MasterDataService>();
            services.AddScoped<IUserService, UserService>();
        }
    }
}
