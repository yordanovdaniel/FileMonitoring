using FileMonitoringApp.Services.Monitoring;
using FileMonitoringApp.Services.Scan;
using FileMonitoringApp.Services.Upload;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FileMonitoringApp.Configuration
{
    internal static class ServiceConfiguration
    {
        public static IServiceCollection Configure(this IServiceCollection services)
        {
            return services
                .AddConfiguration()
                .AddLogging(builder => builder.AddConsole())
                .AddServices();
        }

        private static IServiceCollection AddConfiguration(this IServiceCollection services)
        {
            return services.AddSingleton<IConfiguration>(provider =>
                 new ConfigurationBuilder()
                     .AddJsonFile("appsettings.json")
                     .Build()
             );
        }

        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddSingleton<IFileMonitoringService, FileMonitoringService>();
            
            services.AddSingleton<IFileScanningService, FileSystemScanningService>();
            services.AddSingleton<IFileUploadingService, MOVEitFileUploadingService>();

            return services;
        }
    }
}
