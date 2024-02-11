using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TestWorker.Eventing;
using TestWorker.Http.Options;

namespace TestWorker.Http
{
    public record Response(
         string Value,
         byte[] RawValue,
         bool IsRaw,
         bool IsAssigned,
         Dictionary<string, string> ResponseHeaders,
         CookieCollection ResponseCookies,
         HttpStatusCode StatusCode);

    public interface ITransportHttp<TBody>
    {
        CookieCollection Cookies { get; }
        ITransportHttpOptions Options { get; }
        Encoding TransportEncoding { get; }

        event AsyncWrapperEventHandler<TransportHttpFailEventArgs> OnFail;
        event AsyncWrapperEventHandler<TransportHttpRequestEventArgs> OnRequest;
        event AsyncWrapperEventHandler<TransportHttpResponseEventArgs> OnSuccess;

        Task<Response> Execute(TBody body);
    }
}