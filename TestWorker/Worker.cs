using HttpClientSample.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using TestWorker.Extensions;
using TestWorker.Http;
using TestWorker.Services;

namespace TestWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IHostEnvironment _environment;
        private readonly TransportHttpOptions _options;
        private readonly IServiceProvider _serviceProvider;
        private readonly ICredentialStore _credentialStore;

        public Worker(
            ILogger<Worker> logger, 
            IHostEnvironment environment, 
            IOptions<TransportHttpOptions> options,
            IServiceProvider serviceProvider,
            ICredentialStore credentialStore
            )
        {
            _logger = logger;
            _environment = environment;
            _options = options.Value;
            _serviceProvider = serviceProvider;
            _credentialStore = credentialStore;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            
            // _credentialStore.SetCredential(TransportHttpOptions.NAME, new CredentialPFX());

            using (var scope = _serviceProvider.CreateScope())
            {
                var http = scope.ServiceProvider.GetRequiredService<TransportHttpClient>();
                http.OnResponse += Transport_OnResponse;
                await http.Execute();
            }
            

            TransportHttp2 http2 = new TransportHttp2(
                uri: _options.Uri == null? _options.BaseAddress.ToString():  new Uri(_options.BaseAddress, _options.Uri ).ToString(),
                method: _options.Method,
                headers: _options.Headers)
            { Timeout = _options.Timeout };

            http2.OnResponse += Transport_OnResponse;
            await http2.Execute();


            Console.WriteLine(_environment.EnvironmentName);
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                // await http2.Execute();
                await Task.Delay(3000, stoppingToken);
            }
            
        }

        private void Transport_OnResponse(object sender, TransportHttpResponseEventArgs e)
        {
            Console.WriteLine($"OnResponse {DateTime.Now:O}");
            Console.WriteLine(e.Message.Left(200));
            Console.WriteLine();
        }
    }
}
