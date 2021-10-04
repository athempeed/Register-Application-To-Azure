using Newtonsoft.Json;

namespace ADReg.Models
{
    public class AzureApplicationModel
    {
        [JsonProperty("id")]
        public string ID { get; set; }
        [JsonProperty("appId")]
        public string ApplicationID { get; set; }
        
    }
}
