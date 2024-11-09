using HNOne.Web.Services;
using HNOne.Web.Services.Interfaces;

namespace HNOne.Web.Installers
{
    public class ServiceInstaller : IInstaller
    {
        public void InstallerService(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IMasterDataService, MasterDataService>();
            services.AddScoped<IPersonnelService, PersonnelService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPermissionService, PermissionService>();
        }
    }
}
