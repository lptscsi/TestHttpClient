namespace TestWorker.Http.Extensions
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Security.Cryptography.X509Certificates;
    using TestWorker.Credentials;
    using TestWorker.Services;

    public static class HttpHandlerExt
    {
        public static HttpClientHandler ConfigureSecurity(
            this HttpClientHandler handler,
            string name,
            Uri uri,
            ICredentialStore credentialStore
           )
        {
            if (credentialStore != null && credentialStore.TryGetCredential(name, out var credential))
            {
                switch (credential)
                {
                    case CredentialPFX pfx:
                        {
                            if (pfx.Certificate == null || pfx.Certificate.Length == 0)
                            {
                                throw new ArgumentException($"Неверный Credential: Клиентский сертификат пуст");
                            }

                            X509Certificate2Collection certificates = new X509Certificate2Collection();
                            certificates.Import(pfx.Certificate, pfx.Passphrase, X509KeyStorageFlags.PersistKeySet);
                            handler.ClientCertificates.AddRange(certificates);

                            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                        }
                        break;

                    case CredentialMessageUserName user:
                        {
                            handler.PreAuthenticate = true;
                            CredentialCache creds = new CredentialCache();
                            creds.Add(uri, "Basic", new NetworkCredential(user.UserName, user.Password));
                            handler.Credentials = creds;
                            break;
                        }
                    default:
                        throw new ArgumentException($"Неизвестный тип Credential: {credential.GetType().Name}");
                }
            }

            return handler;
        }
    }
}
