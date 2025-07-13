namespace api_InvoicePortal.Models
{
    public class Invoice
    {
        public int? InvoicePid { get; set; } // Nullable for insert
        public int CompanyPid { get; set; }
        public int LedgerPid { get; set; }
        public required string InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal TotalAmount { get; set; }
        public required string PaymentStatus { get; set; }
        public required List<InvoiceItem> InvoiceDetails { get; set; }
    }

    public class InvoiceItem
    {
        public int ProductPid { get; set; }
        public string Description { get; set; }
        public string HsnCode { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxPerc_Gst { get; set; }
        public decimal Total { get; set; }
    }
}
