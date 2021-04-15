using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TestWorker.Eventing;
using TestWorker.Http.Options;

namespace TestWorker.Http
{
    public interface ITransportHttp<TBody>
    {
        TBody Body { get; }
        CookieCollection Cookies { get; }
        ITransportHttpOptions Options { get; }
        string Request { get; }
        string Response { get; }
        (string Value, byte[] RawValue, bool IsRaw, bool IsAssigned) ResponseBody { get; }
        CookieCollection ResponseCookies { get; }
        Dictionary<string, string> ResponseHeaders { get; }
        HttpStatusCode StatusCode { get; }
        Encoding TransportEncoding { get; }

        event AsyncWrapperEventHandler<TransportHttpFailEventArgs> OnFail;
        event AsyncWrapperEventHandler<TransportHttpRequestEventArgs> OnRequest;
        event AsyncWrapperEventHandler<TransportHttpResponseEventArgs> OnResponse;
        event AsyncWrapperEventHandler<EventArgs> OnSuccess;

        Task<bool> Execute();
    }
}