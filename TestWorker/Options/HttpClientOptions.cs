namespace HttpClientSample.Options
{
    using System;

    public class HttpClientOptions
    {
        public Uri BaseAddress { get; set; }

        public bool? UseCookies { get; set; }

        public TimeSpan Timeout { get; set; }
    }
}
