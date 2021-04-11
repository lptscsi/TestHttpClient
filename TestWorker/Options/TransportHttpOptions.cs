using System;
using System.Collections.Generic;

namespace HttpClientSample.Options
{
    public class TransportHttpOptions : HttpClientOptions
    {
        public const string NAME = "TransportHttp";

        public string UserAgent { get; set; }
        
        public Dictionary<string, string> Headers { get; set; }

        public string Method { get; set; }

        public Uri Uri { get; set; }
    }
}
