using FileMonitoringApp.FileTransferClient;
using FileMonitoringApp.FileTransferClient.Auth;
using FileMonitoringApp.FileTransferClient.Connection;
using FileMonitoringApp.Services.FileHash;
using FileMonitoringApp.Services.Monitoring;
using FileMonitoringApp.Services.Scan;
using FileMonitoringApp.Services.Time;
using FileMonitoringApp.Settings.Monitor;
using FileMonitoringApp.Settings.FileTransfer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FileMonitoringApp.Services.RelativePath;

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
            var configuration = CreateConfiguration();
            services.AddSingleton(configuration);

            services.Configure<MonitorSettings>(configuration.GetSection("Monitor"));
            services.Configure<FileTransferSettings>(configuration.GetSection("FileTransfer"));
            services.Configure<FileTransferAuthSettings>(configuration.GetSection("FileTransfer:Auth"));

            return services;
        }

        private static IConfiguration CreateConfiguration()
        {
            return new ConfigurationBuilder()
                     .AddJsonFile("appsettings.json")
                     .Build();
        }

        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddTransient<IFileMonitoringService, FileMonitoringService>();

            services.AddTransient<IFileTransferClient, MOVEitClient>();
            services.AddTransient<IFileTransferAuthenticator, MOVEitAuthenticator>();
            services.AddTransient<IFileTransferServiceConnection, MOVEitServiceConnection>();

            services.AddTransient<IFileScanningService, FileSystemScanningService>();
            services.AddTransient<IFileTransferClient, MOVEitClient>();
            services.AddTransient<ITimeService, TimeService>();
            services.AddTransient<IFileHashService, Sha1FileHashService>();
            services.AddTransient<IRelativePathService, RelativePathService>();

            return services;
        }
    }
}
