
using HNOne.API.Repositories;
using HNOne.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HNOne.API.Installers
{
    public class RepositoryInstaller : IInstaller
    {
        private const string CONN_NAME = "DbConnection";
        public void InstallerService(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<MasterDbContext>(options =>
            {
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                options.UseSqlServer(configuration.GetConnectionString(CONN_NAME) ?? throw new InvalidOperationException($"Connection string '{CONN_NAME}' not found"));
            }, ServiceLifetime.Scoped);
            services.AddTransient<IDapperDbContext, DapperDbContext>();
            services.AddScoped<IMasterDataRepository, MasterDataRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPersonnelRepository, PersonnelRepository>();

        }
    }
}
