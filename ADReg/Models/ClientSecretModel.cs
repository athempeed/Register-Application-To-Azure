using Newtonsoft.Json;

namespace ADReg.Models
{
    public class ClientSecretModel
    {
        public string CustomKeyIdentifier { get; set; }

        [JsonProperty("keyId")]
        public string KeyID { get; set; }

        [JsonProperty("secretText")]
        public string SecretText { get; set; }
    }
}
