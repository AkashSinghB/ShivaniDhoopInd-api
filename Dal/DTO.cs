using api_InvoicePortal.Models;
using API_TurboeSigner_V2.App_Dal;
using Microsoft.Data.SqlClient;
using System.Data;

namespace api_InvoicePortal.Dal
{
    public class DTO
    {
        readonly string ProcName = "LedgerMasterProc";

        public DTO(IConfiguration _config) { DataClass dataClass = new(_config); }
        public int DataOperations(IConfiguration _config, LedgerModel? LedgerModel, string Action, PidModel? pidModel = null)
        {
            try
            {
                DataClass dataClass = new(_config);
                DataTable dt = new();
                List<SqlParameter> param = new List<SqlParameter>();

                param.Add(new SqlParameter("@action", Action));

                if (Action == "DELETE" && pidModel != null)
                {
                    param.Add(new SqlParameter("@Pid", pidModel.Pid));
                }
                else
                {
                    param.Add(new SqlParameter("@LedgerID", LedgerModel.LedgerID));
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
                    param.Add(new SqlParameter("@IsBankDtl", LedgerModel.PartyDetails.BankDetails.ToUpper() == "YES" ? true : false));

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
                }
                DataSet dt1 = new();
                int res = dataClass.ExecuteNonQuery(ProcName, param);
                return res;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in DataOperations", ex);
            }
        }

       
    }
}
