using Microsoft.Extensions.Configuration;
using System;

namespace TestWorker.Http.Options
{
    public class TransportHttpOptions : HttpClientOptions, ITransportHttpOptions
    {
        public const string NAME = "TransportHttp";

        public string HttpClientName { get; set; }

        public Uri AbsoluteUri 
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Uri))
                {
                    if (string.IsNullOrEmpty(this.BaseAddress))
                    {
                        return new Uri(this.Uri, UriKind.Absolute);
                    }
                    else
                    {
                        return new Uri(new Uri(this.BaseAddress, UriKind.Absolute), this.Uri);
                    }
                }

                if (!string.IsNullOrEmpty(this.BaseAddress))
                {
                    return new Uri(this.BaseAddress, UriKind.Absolute);
                }

                return null;
            }
        }
    }
}

