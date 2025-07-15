using api_InvoicePortal.Models;
using API_TurboeSigner_V2.App_Dal;
using Microsoft.Data.SqlClient;
using System.Data;

namespace api_InvoicePortal.Dal
{
    public class DTO
    {
        DataClass dataClass;
        readonly string ProcNameLedger = "LedgerMasterProc";
        readonly string ProcNameProduct = "ProductMasterProc";
        readonly string ProcNameInvoice = "InvoiceMasterProc";
        readonly string ProcNameLogin = "LoginProc";

        public DTO(IConfiguration _config)
        {
            dataClass = new DataClass(_config);
        }

        public int DataOperationsLedger(LedgerModel? LedgerModel, string Action, int Pid = 0)
        {
            try
            {
                if (LedgerModel == null) return 0;
                List<SqlParameter> param = new();
                param.Add(new SqlParameter("@action", Action));
                param.Add(new SqlParameter("@LedgerPid", Pid));
                param.Add(new SqlParameter("@CompanyName", LedgerModel.CompanyName));
                param.Add(new SqlParameter("@SubHead", LedgerModel.SubHead));
                // Add Party Details
                if (LedgerModel.SubHead.ToLower() == "debtors" || LedgerModel.SubHead.ToLower() == "creditors")
                {
                    //param.Add(new SqlParameter("@PartyID", LedgerModel.PartyDetails.PartyID));
                    param.Add(new SqlParameter("@AddressLine1", LedgerModel.PartyDetails.AddressLine1));
                    param.Add(new SqlParameter("@AddressLine2", LedgerModel.PartyDetails.AddressLine2));
                    param.Add(new SqlParameter("@CityPid", LedgerModel.PartyDetails.City));
                    param.Add(new SqlParameter("@StatePid", LedgerModel.PartyDetails.State));
                    param.Add(new SqlParameter("@PostalCode", LedgerModel.PartyDetails.PostalCode));
                    param.Add(new SqlParameter("@Country", LedgerModel.PartyDetails.Country));
                    param.Add(new SqlParameter("@PhoneNumber", LedgerModel.PartyDetails.PhoneNumber));
                    param.Add(new SqlParameter("@Email", LedgerModel.PartyDetails.Email));
                    param.Add(new SqlParameter("@RegistrationType", LedgerModel.PartyDetails.RegistrationType));
                    param.Add(new SqlParameter("@GSTNumber", LedgerModel.PartyDetails.GSTNumber));
                    param.Add(new SqlParameter("@PANNumber", LedgerModel.PartyDetails.PANNumber));
                    param.Add(new SqlParameter("@IsBankDtl", LedgerModel.PartyDetails.BankDetails));
                }
                // Handling multiple bank details through UDT
                if (LedgerModel?.BankDetailsList != null && LedgerModel.BankDetailsList.Count > 0
                    && (LedgerModel.PartyDetails?.BankDetails == true || LedgerModel.SubHead.ToLower() == "bank"))
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
                int res = dataClass.ExecuteNonQuery(ProcNameLedger, param);
                return res;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in Ledger DataOperations", ex);
            }
        }

        public int DataOperationsProduct(ProductModel? Model, string Action, int Pid = 0)
        {
            try
            {
                if (Model == null) return 0;
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
                int res = dataClass.ExecuteNonQuery(ProcNameProduct, param);
                return res;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in Product DataOperations", ex);
            }
        }

        public int DataOperationsInvoice(Invoice? Model, string Action, int Pid = 0)
        {
            try
            {
                if (Model == null) return 0;
                List<SqlParameter> param =
                [
                    new SqlParameter("@action", Action),
                    new SqlParameter("@Pid", Pid),
                    new SqlParameter("@CompanyPid ", Model.CompanyPid),
                    new SqlParameter("@LedgerPid", Model.LedgerPid),
                    new SqlParameter("@InvoiceNo", Model.InvoiceNo),
                    new SqlParameter("@InvoiceDate", Model.InvoiceDate),
                    new SqlParameter("@TotalAmount", Model.TotalAmount),
                    new SqlParameter("@GSTAmount", Model.GSTAmount),
                    new SqlParameter("@SGSTAmount", Model.SGSTAmount),
                    new SqlParameter("@CGSTAmount", Model.CGSTAmount),
                    new SqlParameter("@IGSTAmount", Model.IGSTAmount),
                    new SqlParameter("@PaymentStatus", Model.PaymentStatus),
                ];
                DataTable dtInvoiceDetails = new();
                dtInvoiceDetails.Columns.Add("ProductPid", typeof(int));
                dtInvoiceDetails.Columns.Add("Description", typeof(string));
                dtInvoiceDetails.Columns.Add("HsnCode", typeof(string));
                dtInvoiceDetails.Columns.Add("Quantity", typeof(decimal));
                dtInvoiceDetails.Columns.Add("UnitPrice", typeof(decimal));
                dtInvoiceDetails.Columns.Add("Total", typeof(decimal));
                dtInvoiceDetails.Columns.Add("TaxPerc_Gst", typeof(decimal));

                foreach (var item in Model.InvoiceDetails)
                {
                    dtInvoiceDetails.Rows.Add(
                        item.ProductPid,
                        item.Description,
                        item.HsnCode,
                        item.Quantity,
                        item.UnitPrice,
                        item.TaxPerc_Gst,
                        item.Total
                    );
                }
                param.Add(new SqlParameter("@Udt_InvoiceDetails", dtInvoiceDetails));
                int res = dataClass.ExecuteNonQuery(ProcNameInvoice, param);
                return res;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in Invoice DataOperations", ex);
            }
        }

        public bool Dto_Login(LoginModel Model, out string msg)
        {
            try
            {
                DataTable dt = new();
                List<SqlParameter> param =
                [
                    new SqlParameter("@LoginId ", Model.Username),
                    //new SqlParameter("@PasswordHash", PasswordHash),
                    //new SqlParameter("@Message", msg),
                ];
                dataClass.GetDatatable(ProcNameLogin, ref dt, param);
                if (dt.Rows.Count > 0)
                {
                    msg = dt.Rows[0]["msg"].ToString() ?? "";
                    if (BCrypt.Net.BCrypt.Verify(Model.Password, dt.Rows[0]["PwdHash"].ToString()))
                    {
                        return true;
                    }
                    else
                    {
                        msg = "Invalid Credentials";
                        return false;
                    }
                }
                else
                {
                    msg = "Something went wrong. Data not found.";
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in Invoice DataOperations", ex);
            }
        }
    }
}
