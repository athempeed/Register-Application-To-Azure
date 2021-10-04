namespace ADReg.Models
{
    public class AzureAdInput
    {
        public string ClientID { get; set; }
        public string ClientSeceret { get; set; }
        public string BaseUrl { get; set; }
        public string GrantType { get; set; }
        public string Scope { get; set; } = ".default";
    }
}
