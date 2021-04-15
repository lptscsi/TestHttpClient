using System;
using System.Collections.Generic;

namespace TestWorker.Http.Options
{
    public interface ITransportHttpOptions
    {
        string HttpClientName { get; }
        string BaseAddress { get; }
        TimeSpan Timeout { get; }
        IDictionary<string, string> Headers { get; }
        string Method { get; }
        string Uri { get; }
        string UserAgent { get; }
        Uri AbsoluteUri { get; }
    }
}