using api_InvoicePortal.Dal;
using api_InvoicePortal.Models;
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
    public class LedgerController : Controller
    {
        private readonly IConfiguration _config;
        private readonly ILogger<LedgerController> _logger;
        private readonly DataClass _dataCls;
        private readonly DTO _dto;
        private readonly string procName = "LedgerMasterProc";

        public LedgerController(IConfiguration config, ILogger<LedgerController> logger)
        {
            _logger = logger;
            _config = config;
            _dataCls = new(_config);
            _dto = new(_config);
        }

        [HttpGet("fetch/{id}")]
        public IActionResult FetchLedger(int id)
        {
            DataSet ds = new();
            List<SqlParameter> param = new();
            param.Add(new SqlParameter("@action", "fetch"));
            param.Add(new SqlParameter("@LedgerPid", id));
            _dataCls.GetDataset(procName, ref ds, param);
            if (ds.Tables[0].Rows.Count == 0) return BadRequest("data not found");

            _logger.LogInformation("Ledger fetched: {LedgerID}", id);
            return Ok(ds);
        }

        [HttpPost("create")]
        public IActionResult InsertLedger(LedgerModel ledger)
        {
            if (ledger == null) return BadRequest();

            int res = _dto.DataOperationsLedger(ledger, "INST");
            if (res > 0)
            {
                _logger.LogInformation("Ledger created: {LedgerName}", ledger.CompanyName);
                var successResponse = new SuccessResponse
                {
                    Result = "Success",
                    Remarks = "Ledger created successfully"
                };

                return Ok(successResponse);
            }
            else
            {
                _logger.LogInformation("Ledger creation Error: {LedgerName}", ledger.CompanyName);
                CommonError Err = new CommonError();
                Err.Error_Msg = "Something went wrong or Data not Found!";
                Err.Error_Code = "600";
                Err.Error_Type = "Data insert failed";
                return BadRequest(Err);
            }
        }

        [HttpPut("update/{id}")]
        public IActionResult UpdateLedger(int id, LedgerModel updatedLedger)
        {
            if (updatedLedger == null) return BadRequest();

            int res = _dto.DataOperationsLedger(updatedLedger, "UPDT", id);
            if (res > 0)
            {
                _logger.LogInformation("Ledger updated: {LedgerName}", updatedLedger.CompanyName);
                dynamic SuccessObj = new JObject();
                SuccessObj.Result = "Success";
                SuccessObj.Remarks = "Ledger updated successfully";
                return Ok(SuccessObj);
            }
            else
            {
                _logger.LogInformation("Ledger updation Error: {LedgerName}", updatedLedger.CompanyName);
                CommonError Err = new CommonError();
                Err.Error_Msg = "Something went wrong or Data not Found!";
                Err.Error_Code = "601";
                Err.Error_Type = "Data updation failed";
                return BadRequest(Err);
            }
        }

        [HttpDelete("del/{id}")]
        public IActionResult DeleteLedger(int id)
        {
            List<SqlParameter> param = [new SqlParameter("@action", "DEL"), new SqlParameter("@LedgerPid", id)];
            int res = _dataCls.ExecuteNonQuery(procName, param);
            if (res > 0)
            {
                _logger.LogInformation("Ledger deleted: {LedgerPid}", id);
                dynamic SuccessObj = new JObject();
                SuccessObj.Result = "Success";
                SuccessObj.Remarks = "Ledger Deleted successfully";
                return Ok(SuccessObj);
            }
            else
            {
                _logger.LogInformation("Ledger deletion Error: {LedgerPid}", id);
                CommonError Err = new CommonError();
                Err.Error_Msg = "Something went wrong or Data not Found!";
                Err.Error_Code = "601";
                Err.Error_Type = "Data deletion failed";
                return BadRequest(Err);
            }
        }

        [HttpGet("fetch/Basedata")]
        public IActionResult FetchBaseLedger()
        {
            DataTable dt = new();
            List<SqlParameter> param = new();
            param.Add(new SqlParameter("@action", "show"));
            _dataCls.GetDatatable("BaseData_LedgerMasterProc", ref dt, param);
            if (dt.Rows.Count == 0) return BadRequest("data not found");

            _logger.LogInformation("Ledger fetched: Basedata");
            return Ok(dt);
        }
    }
}
