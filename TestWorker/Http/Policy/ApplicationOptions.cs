using TestWorker.Http.Options;

namespace TestWorker.Http.Policy
{
    public class ApplicationOptions
    {
        public PolicyOptions Policies { get; set; }

        public TransportHttpOptions TransportHttpClient { get; set; }
    }
}
