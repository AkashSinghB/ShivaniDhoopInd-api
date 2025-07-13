namespace api_InvoicePortal.Models
{
    public class ProductModel
    {
        public int Pid { get; set; }
        public int PartyPid { get; set; }
        public required string ProductName { get; set; }
        public required string HSNCode { get; set; }
        public string? ProductDescription { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal OpeningUnits { get; set; }
        public decimal OpeningBalance { get; set; }
        public bool IsActive { get; set; }
    }
}
