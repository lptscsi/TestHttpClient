using HttpClientSample.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using TestWorker.Extensions;
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
                    services.AddSingleton<ICredentialStore, CredentialStore>();
                    services.Configure<TransportHttpOptions>(hostContext.Configuration.GetSection(TransportHttpOptions.NAME));
                    services.AddTransportHttpClient<TransportHttpOptions>(TransportHttpOptions.NAME, hostContext.Configuration);
                    services.AddHostedService<Worker>();
                    services.AddHostedService<LifetimeEventsHostedService>();
                });
    }
}
