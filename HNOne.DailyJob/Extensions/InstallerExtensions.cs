using HNOne.DailyJob.Installers;

namespace HNOne.DailyJob.Extensions
{
    public static class InstallerExtensions
    {
        public static void InstallerExtensionsInAssembly(this IServiceCollection services, IConfiguration configuration)
        {
            // lấy ra hết tất cả các class trong installer loại trừ Interface & AbsClass
            var installer = typeof(Program).Assembly.ExportedTypes.Where(m => typeof(IInstaller).IsAssignableFrom(m)
                && !m.IsInterface && !m.IsAbstract).Select(Activator.CreateInstance).Cast<IInstaller>().ToList();

            // bắt đầu install service
            installer.ForEach(m => m.InstallerService(services, configuration));
        }
    }
}
