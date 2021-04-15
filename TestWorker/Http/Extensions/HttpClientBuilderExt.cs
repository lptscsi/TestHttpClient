namespace TestWorker.Http.Extensions
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using TestWorker.Http.Policy;

    public static class HttpClientBuilderExt
    {
        public static IHttpClientBuilder AddPolicies(
            this IHttpClientBuilder clientBuilder,
            IConfiguration configuration
           )
        {
            var policyOptions = configuration.Get<PolicyOptions>();

            return clientBuilder.AddPolicyHandler(HttpPolicy.GetTimeout(policyOptions.Timeout))
                                .AddPolicyHandler(HttpPolicy.GetRetryPolicy(policyOptions.HttpRetry))
                                .AddPolicyHandler(HttpPolicy.GetCircuitBreakerPolicy(policyOptions.HttpCircuitBreaker));

        }
    }
}
