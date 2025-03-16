namespace api_InvoicePortal.Models
{
    public class JwtSettings
    {
        public string SecretKey { get; set; }
        public int ExpirationMinutes { get; set; }
    }
}
