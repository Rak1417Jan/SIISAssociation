namespace MVEA.API.Application.DTOs.Response
{    
    public class OtpResponse
    {
        public int OtpId { get; set; }
        public string? MobileNo { get; set; }
        public string? OtpCode { get; set; }
        public DateTime? ExpiresOn { get; set; }
    }
}
