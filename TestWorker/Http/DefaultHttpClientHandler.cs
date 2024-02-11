namespace TestWorker.Http
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using SecurityProtocolType = System.Security.Authentication.SslProtocols;

    public class DefaultHttpClientHandler : HttpClientHandler
    {
        public DefaultHttpClientHandler(int maxConcurrentConnections = 10)
        {
            this.AutomaticDecompression = DecompressionMethods.All;
            this.ServerCertificateCustomValidationCallback = ServerCertificateCustomValidation;
            this.SslProtocols =  SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

            // получаем Cookies в ручную
            this.UseCookies = false;
            this.MaxConnectionsPerServer = Math.Max(maxConcurrentConnections, 2);
        }

        private static bool ServerCertificateCustomValidation(HttpRequestMessage requestMessage, X509Certificate2 certificate, X509Chain chain, SslPolicyErrors sslErrors)
        {
            /*
            // It is possible inpect the certificate provided by server
            Console.WriteLine($"Requested URI: {requestMessage.RequestUri}");
            Console.WriteLine($"Effective date: {certificate.GetEffectiveDateString()}");
            Console.WriteLine($"Exp date: {certificate.GetExpirationDateString()}");
            Console.WriteLine($"Issuer: {certificate.Issuer}");
            Console.WriteLine($"Subject: {certificate.Subject}");

            // Based on the custom logic it is possible to decide whether the client considers certificate valid or not
            Console.WriteLine($"Errors: {sslErrors}");
            return sslErrors == SslPolicyErrors.None;
            */
            return true;
        }
    }
}
