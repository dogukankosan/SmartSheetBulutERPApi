namespace SmartSheetLicenceApi.Models
{
    public class LicenseResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string CompanyName { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; }
    }
}