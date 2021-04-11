namespace TestWorker.Credentials
{
    public class CredentialPFX : Credential
    {
        public byte[] Certificate { get; set; }

        public string Passphrase { get; set; }
    }
}