namespace TestWorker.Http.Extensions
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using System;
    using TestWorker.Http.Options;
    using TestWorker.Http.Policy;
    using TestWorker.Services;

    public static class ServiceCollectionExt
    {
        public static IServiceCollection AddTransportHttpClient<TClientOptions>(
            this IServiceCollection services,
            TClientOptions options,
            IConfiguration configuration
            )
            where TClientOptions : ITransportHttpOptions
        {
            services.TryAddSingleton<ICredentialStore, CredentialStore>();
            services.TryAddSingleton<ITransportHttpOptions>(options);

            return services
                .AddHttpClient(options.HttpClientName, (sp, httpClient) =>
                {
                    if (!string.IsNullOrEmpty(options.BaseAddress))
                    {
                        httpClient.BaseAddress = new Uri(options.BaseAddress);
                    }
                    httpClient.Timeout = options.Timeout.TotalSeconds == 0 ? TimeSpan.FromSeconds(30) : options.Timeout;
                })
                .ConfigurePrimaryHttpMessageHandler(sp => {
                    var credentialStore = sp.GetRequiredService<ICredentialStore>();
                    var handler = new DefaultHttpClientHandler(10);
                    handler.ConfigureSecurity(options.HttpClientName, options.AbsoluteUri, credentialStore);
                    return handler; 
                })
                .SetHandlerLifetime(TimeSpan.FromMinutes(2))
                .AddPolicies(configuration.GetSection(PolicyOptions.PoliciesConfigurationSectionName))
                .Services;
        }
    }
}
