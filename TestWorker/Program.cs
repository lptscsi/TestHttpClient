using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using TestWorker.Http.Extensions;
using TestWorker.Http.Options;
using TestWorker.Services;

namespace TestWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.SetBasePath(Directory.GetCurrentDirectory());
                    configHost.AddJsonFile("hostsettings.json", optional: true);
                    configHost.AddEnvironmentVariables(prefix: "PREFIX_");
                    configHost.AddCommandLine(args);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var options = hostContext.Configuration.GetSection(TransportHttpOptions.NAME).Get<TransportHttpOptions>();
                 
                    services.AddTransportHttpClient<ITransportHttpOptions>(options, hostContext.Configuration);
                    services.AddHostedService<Worker>();
                    services.AddHostedService<LifetimeEventsHostedService>();
                });
    }
}
