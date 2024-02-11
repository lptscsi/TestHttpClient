using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using TestWorker.Eventing;
using TestWorker.Extensions;
using TestWorker.Http.Options;

namespace TestWorker.Http
{
    public class TransportHttp<TBody> : ITransportHttp<TBody>
    {
        public const string Accept = "Accept";
        public const string Authorization = "Authorization";
        public const string ContentTypeKey = "Content-Type";
        public const string SetCookieKey = "Set-Cookie";
        public const string SOAPActionKey = "SOAPAction";
        public const string UserAgentKey = "User-Agent";
        private static readonly CookieCollection EMPTY_COOKIES = new CookieCollection();

        protected List<KeyValuePair<string, string>> headers { get; }

        protected string ContentType { get; }

        protected IHttpClientFactory HttpClientFactory { get; }

        public Encoding TransportEncoding { get; }

        public ITransportHttpOptions Options { get; }

        public CookieCollection Cookies { get; private set; }

     
        #region Events

        /// <summary>
        /// Обработчик успешной отправки сообщения
        /// </summary>
        private AsyncEventingWrapper<TransportHttpResponseEventArgs> onSuccess;

        /// <summary>
        /// Событие успешной отправки сообщения
        /// </summary>
        public event AsyncWrapperEventHandler<TransportHttpResponseEventArgs> OnSuccess
        {
            add => onSuccess.AddHandler(value);
            remove => onSuccess.RemoveHandler(value);
        }

        /// <summary>
        /// Обработчик ошибки отправки сообщения
        /// </summary>
        private AsyncEventingWrapper<TransportHttpFailEventArgs> onFail;

        /// <summary>
        /// Событие ошибки отправки сообщения
        /// </summary>
        public event AsyncWrapperEventHandler<TransportHttpFailEventArgs> OnFail
        {
            add => onFail.AddHandler(value);
            remove => onFail.RemoveHandler(value);
        }

        /// <summary>
        /// Обработчик отправки сообщения
        /// </summary>
        private AsyncEventingWrapper<TransportHttpRequestEventArgs> onRequest;

        /// <summary>
        /// Событие отправки сообщения
        /// </summary>
        public event AsyncWrapperEventHandler<TransportHttpRequestEventArgs> OnRequest
        {
            add => onRequest.AddHandler(value);
            remove => onRequest.RemoveHandler(value);
        }

        #endregion

        public TransportHttp(
            ITransportHttpOptions options,
            IHttpClientFactory httpClientFactory,
            Encoding transportEncoding = null,
            CookieCollection cookies = null)
        {
            this.Options = options;
            this.HttpClientFactory = httpClientFactory;
            this.TransportEncoding = transportEncoding ?? Encoding.UTF8;
            this.Cookies = cookies;
            this.headers = this.Options.Headers?.ToList() ?? new List<KeyValuePair<string, string>>();
            this.ContentType = this.GetContentType(this.headers);
        }


        #region Helpers
        private string GetContentType(List<KeyValuePair<string, string>> headers)
        {
            string contentType = string.Empty;

            if (this.Options.Headers?.TryGetValue(ContentTypeKey, out contentType) == true)
            {
                headers.RemoveAll(kv => kv.Key == ContentTypeKey);
            }
            else
            {
                contentType = MediaTypeNames.Application.Json;
            }

            return contentType;
        }

        #endregion

        protected virtual Task<HttpContent> CreateContent(TBody body)
        {
            return this.CreateContent(body, this.TransportEncoding, this.ContentType);
        }

        protected virtual Task<HttpContent> CreateContent(object data, Encoding encoding, string contentType)
        {
            HttpContent result = null;
            if (data == null)
            {
                return Task.FromResult(result);
            }

            switch (data)
            {
                case string body:
                    {
                        result = new StringContent(body, encoding, contentType);
                    }
                    break;
                case byte[] body:
                    {
                        result = new ByteArrayContent(body);
                        result.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                    }
                    break;
                case Stream body:
                    {
                        result = new StreamContent(body);
                        result.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                    }
                    break;
                default:
                    throw new Exception($"Body с типом {data.GetType().Name} не поддерживается");
            };

            return Task.FromResult(result);
        }

        public virtual async Task<Response> Execute()
        {
            return await this.Execute(body: default);
        }

        public virtual async Task<Response> Execute(TBody body)
        {
            try
            {
                var httpMethod = new HttpMethod(this.Options.Method.ToUpper());

                var request = new HttpRequestMessage() { Method = httpMethod };

                if (string.IsNullOrEmpty(this.Options.BaseAddress) && string.IsNullOrEmpty(this.Options.Uri))
                {
                    throw new Exception("Не заполнены URI для запроса");
                }

                if (!string.IsNullOrEmpty(this.Options.Uri))
                {
                    if (string.IsNullOrEmpty(this.Options.BaseAddress))
                    {
                        request.RequestUri = new Uri(this.Options.Uri, UriKind.Absolute);
                    }
                    else
                    {
                        request.RequestUri = new Uri(this.Options.Uri, UriKind.Relative);
                    }
                }

                if (Cookies?.Any() == true)
                {
                    string cookieValue = string.Join("; ", Cookies.Cast<Cookie>().Select(c => c.ToString()));
                    request.Headers.Add("Cookie", cookieValue);
                }

                headers.ForEach(kvp => { request.Headers.Add(kvp.Key, kvp.Value); });

                HttpContent content = await this.CreateContent(body);

                if (content != null)
                {
                    request.Content = content;
                }

                string requestLog = GetRequestLog(request, body);

                await this.RaiseRequest(this, new TransportHttpRequestEventArgs(requestLog));

                using var client = HttpClientFactory.CreateClient(this.Options.HttpClientName);

                using HttpResponseMessage httpResponse = await client.SendAsync(request);

                HttpStatusCode statusCode = httpResponse.StatusCode;

                httpResponse.EnsureSuccessStatusCode();

                Dictionary<string, string> responseHeaders = GetResponseHeaders(httpResponse);

                CookieCollection responseCookies = GetResponseCookies(httpResponse);

                (string Value, byte[] RawValue, bool IsRaw, bool IsAssigned) data = await GetResponseData(httpResponse);

                string responseLog = GetResponseLog(httpResponse, responseHeaders, data);

                Response res = new Response(data.Value, data.RawValue, data.IsRaw, data.IsAssigned, responseHeaders, responseCookies, statusCode);

                await OnCompleted(res, responseLog);

                return res;
            }
            catch (Exception e)
            {
                await OnFailed(e);
                throw;
            }
        }

        protected virtual async Task OnCompleted(Response res, string responseLog)
        {
            await RaiseSuccess(this, new TransportHttpResponseEventArgs(responseLog));
        }

        protected virtual async Task OnFailed(Exception ex, string responseLog = null)
        {
            await RaiseFail(this, new TransportHttpFailEventArgs(ex.GetFullMessage()));
        }

        protected virtual async Task RaiseRequest(object sender, TransportHttpRequestEventArgs e)
        {
            await onRequest.InvokeAsync(sender, e);
        }

        private async Task RaiseSuccess(object sender, TransportHttpResponseEventArgs e)
        {
            await onSuccess.InvokeAsync(sender, e);
        }

        private async Task RaiseFail(object sender, TransportHttpFailEventArgs e)
        {
            await onFail.InvokeAsync(sender, e);
        }

        private string GetRequestLog(HttpRequestMessage request, TBody body)
        {
            StringBuilder requestLog = new StringBuilder();
            requestLog.AppendFormat($"{request.Method} {request.RequestUri}\r\n");

            foreach (var item in request.Headers)
            {
                string value = item.Value == null ? string.Empty : string.Join(", ", item.Value);
                _ = requestLog.AppendFormat($"{item.Key}: {value}\r\n");
            }

            requestLog.AppendLine();

            if (body != null)
            {
                switch (body)
                {
                    case byte[] _body:
                        {
                            requestLog.AppendLine(Convert.ToBase64String(_body));
                        }
                        break;
                    default:
                        {
                            requestLog.AppendLine(body.ToString());
                        }
                        break;
                };
            }

            return requestLog.ToString();
        }

        #region Response Prosessing

        private Dictionary<string, string> GetResponseHeaders(HttpResponseMessage response)
        {
            var headers = response.Headers.Concat(response.Content.Headers);
            var result = new Dictionary<string, string>();

            foreach (var kvp in headers)
            {
                string value = kvp.Value == null ? string.Empty : string.Join(", ", kvp.Value);
                result.TryAdd(kvp.Key, value);
            }
            return result;
        }

        private CookieCollection GetResponseCookies(HttpResponseMessage response)
        {
            if (response.Headers.TryGetValues(SetCookieKey, out var values))
            {
                CookieContainer container = new CookieContainer();
                Uri pageUri = response.RequestMessage.RequestUri;

                if (values != null)
                {
                    foreach (string cookie in values)
                    {
                        container.SetCookies(pageUri, cookie);
                    }
                }

                return container.GetCookies(pageUri);
            }

            return EMPTY_COOKIES;
        }

        protected virtual async Task<(string Value, byte[] RawValue, bool IsRaw, bool IsAssigned)> GetResponseData(HttpResponseMessage response)
        {
            var contentType = response.Content.Headers.ContentType;
            byte[] bytes;

            if (new string[]
                    {
                        MediaTypeNames.Application.Octet,
                        MediaTypeNames.Application.Pdf,
                        MediaTypeNames.Application.Rtf,
                        MediaTypeNames.Application.Zip,
                    }
                .Contains(contentType.MediaType) || response.Content is StreamContent)
            {
                // получаем ответ в виде байтового массива
                bytes = await response.Content.ReadAsByteArrayAsync();
                return (null, bytes, true, true);
            }
            else
            {
                // преобразуем байты в строку
                bytes = await response.Content.ReadAsByteArrayAsync();
                string str = TransportEncoding.GetString(bytes);
                return (str, null, false, true);
            }
        }

        private string GetResponseLog(
            in HttpResponseMessage response,
            in Dictionary<string, string> headers,
            in (string Value, byte[] RawValue, bool IsRaw, bool IsAssigned) data)
        {
            if (response == null)
            {
                return null;
            }

            StringBuilder responseLog = new StringBuilder();
            responseLog.AppendFormat($"{response.RequestMessage.RequestUri.Scheme.ToUpper()} {response.Version} {(int)response.StatusCode} {response.StatusCode}\r\n");

            if (headers != null)
            {
                foreach (var key in headers.Keys)
                {
                    responseLog.AppendFormat($"{key}: {headers[key]}\r\n");
                }
            }

            if (data.IsAssigned)
            {
                responseLog.AppendLine();
                responseLog.AppendLine(data.IsRaw ? Convert.ToBase64String(data.RawValue) : data.Value);
            }
            return responseLog.ToString();
        }

        #endregion
    }
}
