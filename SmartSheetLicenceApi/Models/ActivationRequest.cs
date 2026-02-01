namespace SmartSheetLicenceApi.Models
{
    public class ActivationRequest
    {
        public string LicenseKey { get; set; }
        public string HardwareId { get; set; }
        public string CompanyName { get; set; }
    }
}