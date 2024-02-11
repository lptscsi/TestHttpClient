using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TestWorker.Extensions;
using TestWorker.Http;
using TestWorker.Http.Options;
using TestWorker.Services;

namespace TestWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IHostEnvironment _environment;
        private readonly ITransportHttpOptions _options;
        private readonly IServiceProvider _serviceProvider;
        private readonly ICredentialStore _credentialStore;
        private readonly IHttpClientFactory _httpClientFactory;

        public Worker(
            ILogger<Worker> logger, 
            IHostEnvironment environment,
            IHttpClientFactory httpClientFactory,
            ITransportHttpOptions options,
            IServiceProvider serviceProvider,
            ICredentialStore credentialStore
            )
        {
            _logger = logger;
            _environment = environment;
            _httpClientFactory = httpClientFactory;
            _options = options;
            _serviceProvider = serviceProvider;
            _credentialStore = credentialStore;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            // _credentialStore.SetCredential(TransportHttpOptions.NAME, new CredentialPFX());

            var http = new TransportHttp<string>(
             _options,
             _httpClientFactory);

            http.OnSuccess += Transport_OnResponse;

            Console.WriteLine(_environment.EnvironmentName);
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await http.Execute();
                await Task.Delay(3000, stoppingToken);
            }
            
        }

        private Task Transport_OnResponse(object sender, TransportHttpResponseEventArgs e)
        {
            byte[] utf8 = Serialize(e.Message ?? "????");
            string message = Deserialize<string>(utf8);
            Console.WriteLine($"OnResponse {DateTime.Now:O}");
            Console.WriteLine(message?.Left(1000)?? "NO Data!!!");
            Console.WriteLine();
            return Task.CompletedTask;
        }

        private static T Deserialize<T>(byte[] bytes)
        {
            return JsonSerializer.Deserialize<T>(bytes);
        }

        private static byte[] Serialize<T>(T value)
        {
            var buffer = new ArrayBufferWriter<byte>();
            using var writer = new Utf8JsonWriter(buffer);
            JsonSerializer.Serialize(writer, value);
            return buffer.WrittenSpan.ToArray();
        }
    }
}
