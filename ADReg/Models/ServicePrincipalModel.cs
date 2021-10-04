using Newtonsoft.Json;
using System.Collections.Generic;

namespace ADReg.Models
{
    public class ServicePrincipalModel
    {
        [JsonProperty("@odata.context")]
        public string ODataContext { get; set; }
        [JsonProperty("value")]
        public List<ServicePrincipalValue> Value { get; set; }
    }

    public class ServicePrincipalValue
    {
        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("appDisplayName")]
        public string AppDisplayName { get; set; }
    }
}
