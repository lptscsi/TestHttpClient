namespace TestWorker.Http
{
    using TestWorker.Credentials;
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using SecurityProtocolType = System.Security.Authentication.SslProtocols;

    public class DefaultHttpClientHandler2 : HttpClientHandler
    {
        public DefaultHttpClientHandler2(
             CredentialPFX pfx = null)
        {
            this.AutomaticDecompression = DecompressionMethods.All;
            this.ServerCertificateCustomValidationCallback = ServerCertificateCustomValidation;
            this.SslProtocols = SecurityProtocolType.Tls
                | SecurityProtocolType.Tls11
                | SecurityProtocolType.Tls12;
            
            // получаем Cookies в ручную
            this.UseCookies = false;
         
            if (pfx != null)
            {
                if (pfx.Certificate == null || pfx.Certificate.Length == 0)
                    throw new ArgumentException($"Неверный Credential: Клиентский сертификат пуст");

                X509Certificate2Collection certificates = new X509Certificate2Collection();
                certificates.Import(pfx.Certificate, pfx.Passphrase, X509KeyStorageFlags.PersistKeySet);
                this.ClientCertificates.AddRange(certificates);
            }
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
