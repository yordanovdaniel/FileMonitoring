using FileMonitoringApp.Configuration;
using FileMonitoringApp.Services.Monitoring;
using Microsoft.Extensions.DependencyInjection;

namespace FileMonitoringApp
{
    internal class Program
    {
        static void Main()
        {
            var serviceProvider = CreateConfiguredServiceProvider();

        }

        private static IServiceProvider CreateConfiguredServiceProvider()
        {
            return new ServiceCollection()
                .Configure()
                .BuildServiceProvider();
        }
    }
}
