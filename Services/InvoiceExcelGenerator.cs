using api_InvoicePortal.Controllers;
using API_TurboeSigner_V2.App_Dal;
using OfficeOpenXml;
using System.Data;
using System.Diagnostics;

namespace api_InvoicePortal.Services
{
    public class InvoiceExcelGenerator
    {
        private readonly IConfiguration _config;
        private readonly ILogger<LedgerController> _logger;
        private readonly DataClass _dataCls;

        public InvoiceExcelGenerator(IConfiguration config, ILogger<LedgerController> logger)
        {
            _logger = logger;
            _config = config;
            _dataCls = new(_config);
        }
        public void MainFun(DataSet ds)
        {
            try
            {
                DataTable dtCompany = ds.Tables[0];
                DataTable dtParty = ds.Tables[1];
                DataTable dtInvLineItem = ds.Tables[2];
                string TemplateFileName = "TemplateInvoice.xlsx",
                RootPath = _config.GetValue<string>("MySetting:RootPath").ToString(),
                    TemplatePath = Path.Combine(RootPath, _config.GetValue<string>("MySetting:TemplatePath").ToString(), TemplateFileName),
                    OutputFileName = "E:\\Akash Singh B\\Desktop\\Personal Work\\api_InvoicePortal\\download\\hell.xlsx";

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using var stream = new FileStream(TemplatePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets[0];

                worksheet.Cells["D2"].Value = dtCompany.Rows[0]["CompanyName"].ToString().ToUpper();
                worksheet.Cells["D4"].Value = dtCompany.Rows[0]["AddressLine1"].ToString() + " " + dtCompany.Rows[0]["AddressLine2"].ToString();
                worksheet.Cells["E6"].Value = dtCompany.Rows[0]["StateName"].ToString() + ", Code: " + dtCompany.Rows[0]["StateCode"].ToString();
                worksheet.Cells["E7"].Value = dtCompany.Rows[0]["PhoneNumber"].ToString();
                worksheet.Cells["E8"].Value = dtCompany.Rows[0]["Email"].ToString();
                worksheet.Cells["E9"].Value = dtCompany.Rows[0]["GSTNumber"].ToString();
                worksheet.Cells["D63"].Value = dtCompany.Rows[0]["PAN"].ToString();
                worksheet.Cells["H64"].Value = "for " + dtCompany.Rows[0]["CompanyName"].ToString().ToUpper();

                //------------ Ship to ----------
                worksheet.Cells["A11"].Value = dtParty.Rows[0]["LedgerName"].ToString().ToUpper();
                worksheet.Cells["A13"].Value = dtParty.Rows[0]["AddressLine1"].ToString() + " " + dtParty.Rows[0]["AddressLine2"].ToString();
                worksheet.Cells["C14"].Value = dtParty.Rows[0]["StateName"].ToString() + ", Code: " + dtParty.Rows[0]["StateCode"].ToString();
                worksheet.Cells["C15"].Value = dtParty.Rows[0]["GSTNumber"].ToString();
                worksheet.Cells["C16"].Value = dtParty.Rows[0]["PhoneNumber"].ToString();
                worksheet.Cells["C17"].Value = dtParty.Rows[0]["Email"].ToString();

                //------------ Bill to ----------
                worksheet.Cells["A19"].Value = dtParty.Rows[0]["LedgerName"].ToString().ToUpper();
                worksheet.Cells["A21"].Value = dtParty.Rows[0]["AddressLine1"].ToString() + " " + dtParty.Rows[0]["AddressLine2"].ToString();
                worksheet.Cells["C22"].Value = dtParty.Rows[0]["StateName"].ToString() + ", Code: " + dtParty.Rows[0]["StateCode"].ToString();
                worksheet.Cells["C23"].Value = dtParty.Rows[0]["GSTNumber"].ToString();
                worksheet.Cells["C24"].Value = dtParty.Rows[0]["PhoneNumber"].ToString();
                worksheet.Cells["C25"].Value = dtParty.Rows[0]["Email"].ToString();

                //------------ Invoice Dtl ----------
                worksheet.Cells["G11"].Value = dtParty.Rows[0]["InvoiceNo"].ToString();
                worksheet.Cells["I11"].Value = Convert.ToDateTime(dtParty.Rows[0]["InvoiceDate"].ToString()).ToString("dd-MMM-yy");
                worksheet.Cells["K49"].Value = "₹ " + dtParty.Rows[0]["TotalAmount"].ToString();

                //------------ Line Items ----------
                int ExlRowNo = 28, SrNo = 1; decimal TotalQty = 0;
                foreach (DataRow dr in dtInvLineItem.Rows)
                {
                    TotalQty += Convert.ToDecimal(dr["Quantity"]);
                    worksheet.Cells["A" + ExlRowNo].Value = SrNo++;
                    worksheet.Cells["B" + ExlRowNo].Value = dr["ProductName"].ToString();
                    worksheet.Cells["F" + ExlRowNo].Value = dr["HsnCode"].ToString();
                    worksheet.Cells["G" + ExlRowNo].Value = Convert.ToDecimal(dr["Quantity"]);
                    worksheet.Cells["H" + ExlRowNo].Value = Convert.ToDecimal(dr["UnitPrice"]);
                    worksheet.Cells["I" + ExlRowNo].Value = "kg";//product Unit
                    worksheet.Cells["J" + ExlRowNo].Value = "";//discount
                    worksheet.Cells["K" + ExlRowNo].Value = Convert.ToDecimal(dr["TotalPrice"]).ToString("D2");

                    worksheet.Cells["B" + (ExlRowNo + 1)].Value = dr["Description"].ToString();

                    ExlRowNo += 2;
                }
                worksheet.Cells["G49"].Value = TotalQty;
                package.SaveAs(OutputFileName);

                ConvertExcelToPdf(OutputFileName, Path.GetDirectoryName(OutputFileName));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public void ConvertExcelToPdf(string inputPath, string outputFolder)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = @"C:\Program Files\LibreOffice\program\soffice.exe", // or full path to soffice.exe
                    Arguments = $"--headless --convert-to pdf \"{inputPath}\" --outdir \"{outputFolder}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            //Console.WriteLine("Output: " + output);
            //Console.WriteLine("Error: " + error);
        }
    }
}
