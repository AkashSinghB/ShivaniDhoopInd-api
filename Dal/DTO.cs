using api_InvoicePortal.Models;
using API_TurboeSigner_V2.App_Dal;
using Microsoft.Data.SqlClient;
using System.Data;

namespace api_InvoicePortal.Dal
{
    public class DTO
    {
        readonly string ProcNameLedger = "LedgerMasterProc";
        readonly string ProcNameProduct = "ProductMasterProc";

        public DTO(IConfiguration _config) { DataClass dataClass = new(_config); }
        public int DataOperationsLedger(IConfiguration _config, LedgerModel? LedgerModel, string Action, int Pid = 0)
        {
            try
            {
                if (LedgerModel == null) return 0;
                DataClass dataClass = new(_config);
                DataTable dt = new();
                List<SqlParameter> param = new();
                param.Add(new SqlParameter("@action", Action));
                param.Add(new SqlParameter("@LedgerPid", Pid));
                param.Add(new SqlParameter("@CompanyName", LedgerModel.CompanyName));
                param.Add(new SqlParameter("@SubHead", LedgerModel.SubHead));
                // Add Party Details
                //param.Add(new SqlParameter("@PartyID", LedgerModel.PartyDetails.PartyID));
                param.Add(new SqlParameter("@AddressLine1", LedgerModel.PartyDetails.AddressLine1));
                param.Add(new SqlParameter("@AddressLine2", LedgerModel.PartyDetails.AddressLine2));
                param.Add(new SqlParameter("@City", LedgerModel.PartyDetails.City));
                param.Add(new SqlParameter("@State", LedgerModel.PartyDetails.State));
                param.Add(new SqlParameter("@PostalCode", LedgerModel.PartyDetails.PostalCode));
                param.Add(new SqlParameter("@Country", LedgerModel.PartyDetails.Country));
                param.Add(new SqlParameter("@PhoneNumber", LedgerModel.PartyDetails.PhoneNumber));
                param.Add(new SqlParameter("@Email", LedgerModel.PartyDetails.Email));
                param.Add(new SqlParameter("@RegistrationType", LedgerModel.PartyDetails.RegistrationType));
                param.Add(new SqlParameter("@GSTNumber", LedgerModel.PartyDetails.GSTNumber));
                param.Add(new SqlParameter("@PANNumber", LedgerModel.PartyDetails.PANNumber));
                param.Add(new SqlParameter("@IsBankDtl", LedgerModel.PartyDetails.BankDetails));

                // Handling multiple bank details through UDT
                if (LedgerModel?.BankDetailsList != null && LedgerModel.BankDetailsList.Count > 0)
                {
                    DataTable bankDetailsTable = new DataTable();
                    bankDetailsTable.Columns.Add("AccountType", typeof(string));
                    bankDetailsTable.Columns.Add("AccountNumber", typeof(string));
                    bankDetailsTable.Columns.Add("BankName", typeof(string));
                    bankDetailsTable.Columns.Add("BankBranch", typeof(string));
                    bankDetailsTable.Columns.Add("IFSCCode", typeof(string));
                    bankDetailsTable.Columns.Add("MICRCode", typeof(string));
                    bankDetailsTable.Columns.Add("AccountHolderName", typeof(string));

                    foreach (var bank in LedgerModel.BankDetailsList)
                    {
                        bankDetailsTable.Rows.Add(
                            bank.AccountType ?? string.Empty,
                            bank.AccountNumber ?? string.Empty,
                            bank.BankName ?? string.Empty,
                            bank.BankBranch ?? string.Empty,
                            bank.IFSCCode ?? string.Empty,
                            bank.MICRCode ?? string.Empty,
                            bank.AccountHolderName ?? string.Empty
                        );
                    }

                    param.Add(new SqlParameter("@BankDetailsTable", bankDetailsTable));
                }
                DataSet dt1 = new();
                int res = dataClass.ExecuteNonQuery(ProcNameLedger, param);
                return res;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in DataOperations", ex);
            }
        }

        public int DataOperationsProduct(IConfiguration _config, ProductModel? Model, string Action, int Pid = 0)
        {
            try
            {
                if(Model == null) return 0;
                DataClass dataClass = new(_config);
                DataTable dt = new();
                List<SqlParameter> param = new();
                param.Add(new SqlParameter("@action", Action));
                param.Add(new SqlParameter("@Pid", Pid));
                param.Add(new SqlParameter("@CompanyPid ", Model.PartyPid));
                param.Add(new SqlParameter("@ProductName", Model.ProductName));
                param.Add(new SqlParameter("@HSNCode", Model.HSNCode));
                param.Add(new SqlParameter("@ProductDescription", Model.ProductDescription));
                param.Add(new SqlParameter("@UnitPrice", Model.UnitPrice));
                param.Add(new SqlParameter("@OpeningUnits", Model.OpeningUnits));
                param.Add(new SqlParameter("@OpeningBalance", Model.OpeningBalance));
                param.Add(new SqlParameter("@IsActive", Model.IsActive));
                DataSet dt1 = new();
                int res = dataClass.ExecuteNonQuery(ProcNameProduct, param);
                return res;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in DataOperations", ex);
            }
        }
    }
}
