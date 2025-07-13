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
    public class ProductController : Controller
    {
        private readonly IConfiguration _config;
        private readonly ILogger<LedgerController> _logger;
        private readonly DataClass _dataCls;
        private readonly DTO _dto;
        private readonly string procName = "ProductMasterProc";

        public ProductController(IConfiguration config, ILogger<LedgerController> logger)
        {
            _logger = logger;
            _config = config;
            _dataCls = new(_config);
            _dto = new(_config);
        }

        [HttpGet("fetch/{id}")]
        public IActionResult FetchProduct(int id)
        {
            DataSet ds = new();
            List<SqlParameter> param = new();
            param.Add(new SqlParameter("@action", "fetch"));
            param.Add(new SqlParameter("@Pid", id));
            _dataCls.GetDataset(procName, ref ds, param);
            if (ds.Tables[0].Rows.Count == 0) return BadRequest("data not found");

            _logger.LogInformation("Product fetched: {ProductID}", id);
            return Ok(ds);
        }

        [HttpPost("create")]
        public IActionResult InsertProdut(ProductModel model)
        {
            if (model == null) return BadRequest();

            int res = _dto.DataOperationsProduct(model, "INST");
            if (res > 0)
            {
                _logger.LogInformation("Product created: {productName}", model.ProductName);
                var successResponse = new SuccessResponse
                {
                    Result = "Success",
                    Remarks = "Product created successfully"
                };

                return Ok(successResponse);
            }
            else
            {
                _logger.LogInformation("Product creation Error: {ProductName}", model.ProductName);
                CommonError Err = new CommonError();
                Err.Error_Msg = "Something went wrong or Data not Found!";
                Err.Error_Code = "600";
                Err.Error_Type = "Data insert failed";
                return BadRequest(Err);
            }
        }

        [HttpPut("update/{id}")]
        public IActionResult UpdateProduct(int id, ProductModel model)
        {
            if (model == null) return BadRequest();

            int res = _dto.DataOperationsProduct(model, "UPDT", id);
            if (res > 0)
            {
                _logger.LogInformation("Product updated: {ProductName}", model.ProductName);
                dynamic SuccessObj = new JObject();
                SuccessObj.Result = "Success";
                SuccessObj.Remarks = "Product updated successfully";
                return Ok(SuccessObj);
            }
            else
            {
                _logger.LogInformation("Product udpation Error: {ProductName}", model.ProductName);
                CommonError Err = new CommonError();
                Err.Error_Msg = "Something went wrong or Data not Found!";
                Err.Error_Code = "601";
                Err.Error_Type = "Data updation failed";
                return BadRequest(Err);
            }
        }

        [HttpDelete("del/{id}")]
        public IActionResult DeleteProduct(int id)
        {
            List<SqlParameter> param = [new SqlParameter("@action", "DEL"), new SqlParameter("@Pid", id)];
            int res = _dataCls.ExecuteNonQuery(procName, param);
            if (res > 0)
            {
                _logger.LogInformation("Product deleted: {ProductPid}", id);
                dynamic SuccessObj = new JObject();
                SuccessObj.Result = "Success";
                SuccessObj.Remarks = "Product Deleted successfully";
                return Ok(SuccessObj);
            }
            else
            {
                _logger.LogInformation("Product deletion Error: {ProductPid}", id);
                CommonError Err = new CommonError();
                Err.Error_Msg = "Something went wrong or Data not Found!";
                Err.Error_Code = "601";
                Err.Error_Type = "Data deletion failed";
                return BadRequest(Err);
            }
        }

        [HttpGet("fetch/Basedata")]
        public IActionResult FetchBaseProduct()
        {
            DataTable dt = new();
            List<SqlParameter> param = new();
            param.Add(new SqlParameter("@action", "show"));
            _dataCls.GetDatatable("BaseData_ProductMasterProc", ref dt, param);
            if (dt.Rows.Count == 0) return BadRequest("data not found");

            _logger.LogInformation("All Products fetched: Basedata");
            return Ok(dt);
        }
    }
}
