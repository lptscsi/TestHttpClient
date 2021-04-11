using TestWorker.Credentials;
using System;
using System.Collections.Concurrent;

namespace TestWorker.Services
{
    public class CredentialStore : ICredentialStore, IDisposable
    {

        private ConcurrentDictionary<string, Credential> _credentials = new ConcurrentDictionary<string, Credential>();

        public CredentialStore()
        {

        }

        public bool TryGetCredential(string key, out Credential credential)
        {
            return _credentials.TryGetValue(key, out credential);
        }

        public Credential SetCredential(string key, Credential credential)
        {
            return _credentials.AddOrUpdate(key, credential, (k, old) => credential);
        }

        public void Dispose()
        {
            _credentials.Clear();
        }
    }
}
