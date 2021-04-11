using Newtonsoft.Json;

namespace TestWorker.Credentials
{
    public class Credential
    {
        public static T Parse<T>(string data)
            where T : Credential
        {
            if (string.IsNullOrEmpty(data))
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(data);
        }
    }
}