using TestWorker.Credentials;

namespace TestWorker.Services
{
    public interface ICredentialStore
    {
        Credential SetCredential(string key, Credential credential);
        bool TryGetCredential(string key, out Credential credential);
    }
}