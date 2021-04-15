namespace TestWorker.Http.Options
{
    using System;
    using System.Collections.Generic;

    public class HttpClientOptions
    {
        public string BaseAddress { get; set; }

        public TimeSpan Timeout { get; set; }
        
        public string UserAgent { get; set; }

        public IDictionary<string, string> Headers { get; set; }

        public string Method { get; set; }

        public string Uri { get; set; }
    }
}
