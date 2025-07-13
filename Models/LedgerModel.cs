namespace api_InvoicePortal.Models
{
    public class LedgerModel
    {
        public int LedgerID { get; set; }
        public required string CompanyName { get; set; }
        public required string SubHead { get; set; }

        public PartyDetailsModel? PartyDetails { get; set; }

        public List<BankDetailsModel>? BankDetailsList { get; set; }
    }

    public class PartyDetailsModel
    {
        public int PartyID { get; set; }
        public int LedgerID { get; set; }
        public required string AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public required string City { get; set; }
        public required string State { get; set; }
        public required string PostalCode { get; set; }
        public required string Country { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Email { get; set; }
        public required string RegistrationType { get; set; }
        public string? GSTNumber { get; set; }
        public string? PANNumber { get; set; }
        public bool? BankDetails { get; set; }
    }

    public class BankDetailsModel
    {
        public int? BankID { get; set; }
        public int? LedgerID { get; set; }
        public string AccountNumber { get; set; }
        public string IFSCCode { get; set; }
        public string BankBranch { get; set; }
        public string MICRCode { get; set; }
        public string AccountType { get; set; }
        public string BankName { get; set; }
        public string AccountHolderName { get; set; }
    }

    public class PidModel
    {
        public Int64 Pid { get; set; }
    }
}
