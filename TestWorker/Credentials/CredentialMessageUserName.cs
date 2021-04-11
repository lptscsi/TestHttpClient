namespace TestWorker.Credentials
{
    public class CredentialMessageUserName : Credential
    {
        /// <summary>
        /// Uri
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Пароль пользователя
        /// </summary>
        public string Password { get; set; }
    }
}