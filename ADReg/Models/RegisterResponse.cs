namespace ADReg.Models
{
    public class RegisterResponse
    {
        public string ApplicationID { get; set; }
        public string ClientSecret { get; set; }
        public string TenantID { get; set; }
        public string Scope { get; set; } = "api://ec9c5bb5-261d-4086-b111-b85ba91a8a21/.default";

        public string ServicePrincipal { get; set; }

    }
}
