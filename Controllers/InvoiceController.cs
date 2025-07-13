using api_InvoicePortal.Dal;
using api_InvoicePortal.Models;
using api_InvoicePortal.Services;
using API_TurboeSigner_V2.App_Dal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using System.Data;

namespace api_InvoicePortal.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InvoiceController : Controller
    {
        private readonly IConfiguration _config;
        private readonly ILogger<LedgerController> _logger;
        private readonly DataClass _dataCls;
        private readonly DTO _dto;
        private readonly string procName = "InvoiceMasterProc";

        public InvoiceController(IConfiguration config, ILogger<LedgerController> logger)
        {
            _logger = logger;
            _config = config;
            _dataCls = new(_config);
            _dto = new(_config);
        }

        [HttpGet("sales/fetch/{id}")]
        public IActionResult FetchSalesInvoice(int id)
        {
            DataSet ds = new();
            List<SqlParameter> param = new();
            param.Add(new SqlParameter("@action", "fetch"));
            param.Add(new SqlParameter("@Pid", id));
            _dataCls.GetDataset(procName, ref ds, param);
            if (ds.Tables[0].Rows.Count == 0) return BadRequest("data not found");

            _logger.LogInformation("Ledger fetched: {LedgerID}", id);
            return Ok(ds);
        }

        [HttpGet("fetchParty/{id}")]
        public IActionResult FetchPartyDtl(int id)
        {
            DataSet ds = new();
            List<SqlParameter> param = new();
            param.Add(new SqlParameter("@action", "GetPartyDtl"));
            param.Add(new SqlParameter("@LedgerPid", id));
            _dataCls.GetDataset(procName, ref ds, param);
            if (ds.Tables[0].Rows.Count == 0) return BadRequest("data not found");

            _logger.LogInformation("Party Details: {PartyPid}", id);
            return Ok(ds);
        }

        [HttpGet("sales/fetch/Basedata")]
        public IActionResult FetchBaseLedger()
        {
            DataTable dt = new();
            List<SqlParameter> param = new();
            param.Add(new SqlParameter("@action", "show"));
            _dataCls.GetDatatable("BaseData_InvoiceMasterProc", ref dt, param);
            if (dt == null) return BadRequest("data not found");

            _logger.LogInformation("sales invoice fetched: Basedata");
            return Ok(dt);
        }

        [HttpPost("sales/create")]
        public IActionResult CreateSaleInvoice(Invoice model)
        {
            if (model == null) return BadRequest();

            int res = _dto.DataOperationsInvoice(model, "INST");
            if (res > 0)
            {
                _logger.LogInformation("Invoice created: {InvoiceNo}", model.InvoiceNo);
                var successResponse = new SuccessResponse
                {
                    Result = "Success",
                    Remarks = "Invoice created successfully"
                };

                return Ok(successResponse);
            }
            else
            {
                _logger.LogInformation("Invoice creation Error: {InvoiceNo}", model.InvoiceNo);
                CommonError Err = new CommonError();
                Err.Error_Msg = "Something went wrong or Data not Found!";
                Err.Error_Code = "600";
                Err.Error_Type = "Data insert failed";
                return BadRequest(Err);
            }
        }

        [HttpPut("sales/update/{id}")]
        public IActionResult UpdateSaleInvoice(int id, Invoice model)
        {
            if (model == null) return BadRequest();

            int res = _dto.DataOperationsInvoice(model, "UPDT", id);
            if (res > 0)
            {
                _logger.LogInformation("Invoice updated: {InvoiceNo}", model.InvoiceNo);
                var successResponse = new SuccessResponse
                {
                    Result = "Success",
                    Remarks = "Invoice udpated successfully."
                };

                return Ok(successResponse);
            }
            else
            {
                _logger.LogInformation("Invoice udpation Error: {InvoiceNo}", model.InvoiceNo);
                CommonError Err = new CommonError();
                Err.Error_Msg = "Something went wrong or Data not Found!";
                Err.Error_Code = "600";
                Err.Error_Type = "Data update failed";
                return BadRequest(Err);
            }
        }

        [HttpDelete("sales/del/{id}")]
        public IActionResult DeleteSaleInvoice(int id)
        {
            List<SqlParameter> param = [new SqlParameter("@action", "DEL"), new SqlParameter("@Pid", id)];
            int res = _dataCls.ExecuteNonQuery(procName, param);
            if (res > 0)
            {
                _logger.LogInformation("Invoice deleted: {InvoicePid}", id);
                dynamic SuccessObj = new JObject();
                SuccessObj.Result = "Success";
                SuccessObj.Remarks = "Invoice Deleted successfully";
                return Ok(SuccessObj);
            }
            else
            {
                _logger.LogInformation("Invoice deletion Error: {InvoicePid}", id);
                CommonError Err = new CommonError();
                Err.Error_Msg = "Something went wrong or Data not Found!";
                Err.Error_Code = "601";
                Err.Error_Type = "Data deletion failed";
                return BadRequest(Err);
            }
        }

        [HttpGet("sales/print/{id}")]
        public IActionResult CreateInvPdf(int id)
        {
            DataSet ds = new();
            List<SqlParameter> param = [new SqlParameter("@action", "GetInvDtl"), new SqlParameter("@Pid", id)];
            _dataCls.GetDataset(procName, ref ds, param);
            if (ds.Tables[0].Rows.Count > 0)
            {
                InvoiceExcelGenerator invoiceExcelGenerator = new(_config, _logger);
                invoiceExcelGenerator.MainFun(ds);
                _logger.LogInformation("Invoice deleted: {InvoicePid}", id);
                dynamic SuccessObj = new JObject();
                SuccessObj.Result = "Success";
                SuccessObj.Remarks = "Invoice Deleted successfully";
                return Ok(SuccessObj);
            }
            else
            {
                _logger.LogInformation("Invoice deletion Error: {InvoicePid}", id);
                CommonError Err = new CommonError();
                Err.Error_Msg = "Something went wrong or Data not Found!";
                Err.Error_Code = "601";
                Err.Error_Type = "Data deletion failed";
                return BadRequest(Err);
            }
        }
    }
}
