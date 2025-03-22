using FileMonitoringApp.Services.Monitoring;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileMonitoringApp.Configuration
{
    internal static class ServiceConfiguration
    {
        public static IServiceCollection Configure(this IServiceCollection services)
        {
            return services
                .AddConfiguration();
        }

        private static IServiceCollection AddConfiguration(this IServiceCollection services)
        {
            return services.AddSingleton<IConfiguration>(provider =>
                 new ConfigurationBuilder()
                     .AddJsonFile("appsettings.json")
                     .Build()
             );
        }

    }
}
