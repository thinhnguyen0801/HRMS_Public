
using Blazored.Toast;
using DevExpress.Blazor;
using HNOne.Web.Services.Interfaces;
using HNOne.Web.Services;
using Blazored.LocalStorage;
using HNOne.Common;

namespace HNOne.Web.Installers
{
    public class ControlInstaller : IInstaller
    {
        public void InstallerService(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDevExpressBlazor(configure => configure.BootstrapVersion = BootstrapVersion.v5);
            services.AddBlazoredLocalStorage();
            services.AddBlazoredToast();

            services.AddSingleton<IDateTimeHelper, DateTimeHelper>();
            services.AddSingleton<IEncryptHelper, EncryptHelper>();
            services.AddTransient<IProgressService, ProgressService>();
            services.AddScoped<ILoadingService, LoadingService>();
            
        }
    }
}
