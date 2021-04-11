namespace TestWorker.Extensions
{
    using HttpClientSample.Options;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Options;
    using System;
    using TestWorker.Http;
    using TestWorker.Services;

    public static class ServiceCollectionExt
    {
        public static IServiceCollection AddTransportHttpClient<TClientOptions>(
            this IServiceCollection services,
            string name,
            IConfiguration configuration
            )
            where TClientOptions : TransportHttpOptions, new()
        {
            var section = configuration.GetSection(name);
            // var options = section.Get<TransportHttpOptions>();

            services.TryAddSingleton<ICredentialStore, CredentialStore>();
            services.TryAddTransient<TransportHttpHandler>();
            services.TryAddTransient<TransportHttpClient>();

            return services
                .Configure<TClientOptions>(section)
                .AddHttpClient(name, (sp, httpClient) =>
                {
                    var options = sp.GetRequiredService<IOptions<TClientOptions>>().Value;
                    httpClient.BaseAddress = options.BaseAddress;
                    httpClient.Timeout = options.Timeout;
                })
                .ConfigurePrimaryHttpMessageHandler(sp => {
                    var options = sp.GetRequiredService<IOptions<TClientOptions>>().Value;
                    var credentials = sp.GetRequiredService<ICredentialStore>();
                    return new DefaultHttpClientHandler(TransportHttpOptions.NAME, options, credentials); 
                })
                .AddHttpMessageHandler<TransportHttpHandler>()
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddPolicies(configuration.GetSection(PolicyOptions.PoliciesConfigurationSectionName))
                .Services;
        }
    }
}
