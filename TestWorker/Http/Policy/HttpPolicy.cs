using Polly;
using Polly.Extensions.Http;
using System;
using System.Net.Http;

namespace TestWorker.Http.Policy
{
    internal static class HttpPolicy
    {
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(RetryPolicyOptions options)
        {
            Random jitterer = new Random();
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(options.Count, 
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(options.BackoffPower, retryAttempt))
                                  + TimeSpan.FromMilliseconds(jitterer.Next(0, 100))
                );
        }


        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(CircuitBreakerPolicyOptions options)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(options.ExceptionsAllowedBeforeBreaking, options.DurationOfBreak);
        }


        public static IAsyncPolicy<HttpResponseMessage> GetTimeout(TimeSpan time)
        {
           return Polly.Policy.TimeoutAsync<HttpResponseMessage>(time);
        }
    }
}
