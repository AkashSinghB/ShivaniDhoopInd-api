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
        public byte[] MainFun(DataSet ds, string OutputFileName)
        {
            try
            {
                DataTable dtCompany = ds.Tables[0];
                DataTable dtParty = ds.Tables[1];
                DataTable dtInvLineItem = ds.Tables[2];
                DataTable distinctHsnCode = ds.Tables[3];
                //DataTable distinctHsnCode = ds.Tables[2].AsEnumerable().GroupBy(row => row["HsnCode"]).Select(g => g.First()).CopyToDataTable();

                string TemplateFileName = "TemplateInvoice.xlsx",

                    RootPath = _config.GetValue<string>("MySetting:RootPath").ToString(),
                    TemplatePath = Path.Combine(RootPath, _config.GetValue<string>("MySetting:TemplatePath").ToString(), TemplateFileName),
                    OutputFilePath = Path.Combine(RootPath, "download", OutputFileName + ".xlsx");

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using var stream = new FileStream(TemplatePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var package = new ExcelPackage(stream);
                int InvTaxType = 0;
                if (dtCompany.Rows[0]["StateCode"].ToString() == dtParty.Rows[0]["StateCode"].ToString())
                {
                    InvTaxType = 0; //if same state code then CGST & SGST
                    package.Workbook.Worksheets.Delete("Sheet2");
                }
                else
                {
                    InvTaxType = 1;  //else IGST
                    package.Workbook.Worksheets.Delete("Sheet1");
                }

                var worksheet = package.Workbook.Worksheets[0];
                #region CompanyDetails
                worksheet.Cells["D2"].Value = dtCompany.Rows[0]["CompanyName"].ToString().ToUpper();
                worksheet.Cells["D4"].Value = dtCompany.Rows[0]["AddressLine1"].ToString() + " " + dtCompany.Rows[0]["AddressLine2"].ToString();
                worksheet.Cells["E6"].Value = dtCompany.Rows[0]["StateName"].ToString() + ", Code: " + dtCompany.Rows[0]["StateCode"].ToString();
                worksheet.Cells["E7"].Value = dtCompany.Rows[0]["PhoneNumber"].ToString();
                worksheet.Cells["E8"].Value = dtCompany.Rows[0]["Email"].ToString();
                worksheet.Cells["E9"].Value = dtCompany.Rows[0]["GSTNumber"].ToString();
                worksheet.Cells["D57"].Value = dtCompany.Rows[0]["PAN"].ToString();
                worksheet.Cells["H58"].Value = "for " + dtCompany.Rows[0]["CompanyName"].ToString().ToUpper();
                #endregion

                //------------ Ship to ----------
                #region Ship To
                worksheet.Cells["A11"].Value = dtParty.Rows[0]["LedgerName"].ToString().ToUpper();
                worksheet.Cells["A13"].Value = dtParty.Rows[0]["AddressLine1"].ToString() + " " + dtParty.Rows[0]["AddressLine2"].ToString();
                worksheet.Cells["C14"].Value = dtParty.Rows[0]["StateName"].ToString() + ", Code: " + dtParty.Rows[0]["StateCode"].ToString();
                worksheet.Cells["C15"].Value = dtParty.Rows[0]["GSTNumber"].ToString();
                worksheet.Cells["C16"].Value = dtParty.Rows[0]["PhoneNumber"].ToString();
                worksheet.Cells["C17"].Value = dtParty.Rows[0]["Email"].ToString();
                #endregion
                #region Bill To
                //------------ Bill to ----------
                worksheet.Cells["A19"].Value = dtParty.Rows[0]["LedgerName"].ToString().ToUpper();
                worksheet.Cells["A21"].Value = dtParty.Rows[0]["AddressLine1"].ToString() + " " + dtParty.Rows[0]["AddressLine2"].ToString();
                worksheet.Cells["C22"].Value = dtParty.Rows[0]["StateName"].ToString() + ", Code: " + dtParty.Rows[0]["StateCode"].ToString();
                worksheet.Cells["C23"].Value = dtParty.Rows[0]["GSTNumber"].ToString();
                worksheet.Cells["C24"].Value = dtParty.Rows[0]["PhoneNumber"].ToString();
                worksheet.Cells["C25"].Value = dtParty.Rows[0]["Email"].ToString();
                #endregion
                //------------ Invoice Dtl ----------
                #region Invoice Dtl
                worksheet.Cells["G11"].Value = dtParty.Rows[0]["InvoiceNo"].ToString();
                worksheet.Cells["I11"].Value = Convert.ToDateTime(dtParty.Rows[0]["InvoiceDate"].ToString()).ToString("dd-MMM-yy");
                //worksheet.Cells["K44"].Value = "₹ " + dtParty.Rows[0]["TotalAmount"].ToString();
                #endregion
                //------------ Line Items ---------- 
                #region LineItems
                int ExlRowNo = 28, SrNo = 1; decimal TotalQty = 0, TotalChargeableAmt = 0;
                foreach (DataRow dr in dtInvLineItem.Rows) //when line item is greate than 6 then page break should happen
                {
                    TotalQty += Convert.ToDecimal(dr["Quantity"]);
                    worksheet.Cells["A" + ExlRowNo].Value = SrNo++;
                    worksheet.Cells["B" + ExlRowNo].Value = dr["ProductName"].ToString();
                    worksheet.Cells["F" + ExlRowNo].Value = dr["HsnCode"].ToString();
                    worksheet.Cells["G" + ExlRowNo].Value = Convert.ToDecimal(dr["Quantity"]);
                    worksheet.Cells["H" + ExlRowNo].Value = Convert.ToDecimal(dr["UnitPrice"]);
                    worksheet.Cells["I" + ExlRowNo].Value = "kg";//product Unit
                    worksheet.Cells["J" + ExlRowNo].Value = "";//discount
                    worksheet.Cells["K" + ExlRowNo].Value = Convert.ToDecimal(dr["TotalPrice"]).ToString("F2");

                    worksheet.Cells["B" + (ExlRowNo + 1)].Value = dr["Description"].ToString();

                    ExlRowNo += 2;
                }
                worksheet.Cells["G44"].Value = TotalQty;
                worksheet.Cells["K" + ExlRowNo].Value = dtParty.Rows[0]["TotalAmount"].ToString();
                worksheet.Cells["K" + ExlRowNo].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                if (InvTaxType == 0) //sgst cgst
                {
                    TotalChargeableAmt = Convert.ToDecimal(dtParty.Rows[0]["TotalAmount"]) + Convert.ToDecimal(dtParty.Rows[0]["CGSTAmount"]) + Convert.ToDecimal(dtParty.Rows[0]["CGSTAmount"]);
                    worksheet.Cells["B" + (ExlRowNo + 1)].Value = "Output CSGT";
                    worksheet.Cells["B" + (ExlRowNo + 1)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    worksheet.Cells["K" + (ExlRowNo + 1)].Value = Convert.ToDecimal(dtParty.Rows[0]["CGSTAmount"]).ToString("F2");
                    worksheet.Cells["B" + (ExlRowNo + 2)].Value = "Output SGST";
                    worksheet.Cells["B" + (ExlRowNo + 2)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    worksheet.Cells["K" + (ExlRowNo + 2)].Value = Convert.ToDecimal(dtParty.Rows[0]["SGSTAmount"]).ToString("F2");
                }
                else//igst
                {
                    TotalChargeableAmt = Convert.ToDecimal(dtParty.Rows[0]["TotalAmount"]) + Convert.ToDecimal(dtParty.Rows[0]["IGSTAmount"]);
                    worksheet.Cells["B" + (ExlRowNo + 1)].Value = "Output IGST";
                    worksheet.Cells["B" + (ExlRowNo + 1)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    worksheet.Cells["K" + (ExlRowNo + 1)].Value = Convert.ToDecimal(dtParty.Rows[0]["IGSTAmount"]).ToString("F2");
                }
                worksheet.Cells["K44"].Value = TotalChargeableAmt.ToString("F2");
                worksheet.Cells["A46"].Value = "INR " + NumToWordsConverter.AmountToWordsIndianFormat(TotalChargeableAmt);
                #endregion
                //tax details
                #region Tax Details
                ExlRowNo = 49;
                decimal TaxableVal = 0, GstRate = 0, GstAmt = 0, TotalGstAmt = 0, TotalTaxableVal = 0;

                foreach (DataRow dr in distinctHsnCode.Rows)
                {
                    TaxableVal = Convert.ToDecimal(dr["TaxableValue"]);
                    GstRate = Convert.ToDecimal(dr["TaxPerc_GST"]);
                    GstAmt = (TaxableVal * GstRate) / 100;
                    TotalGstAmt += GstAmt;
                    TotalTaxableVal += TaxableVal;
                    worksheet.Cells["A" + ExlRowNo].Value = dr["HsnCode"].ToString();
                    if (InvTaxType == 0) //sgst cgst
                    {
                        worksheet.Cells["E" + ExlRowNo].Value = TaxableVal.ToString("F2");
                        worksheet.Cells["F" + ExlRowNo].Value = (GstRate / 2) + "%";
                        worksheet.Cells["G" + ExlRowNo].Value = (GstAmt / 2).ToString("F2");
                        worksheet.Cells["H" + ExlRowNo].Value = (GstRate / 2) + "%";
                        worksheet.Cells["I" + ExlRowNo].Value = (GstAmt / 2).ToString("F2");
                        worksheet.Cells["K" + ExlRowNo].Value = GstAmt.ToString("F2");
                    }
                    else //igst
                    {
                        worksheet.Cells["G" + ExlRowNo].Value = TaxableVal.ToString("F2");
                        worksheet.Cells["H" + ExlRowNo].Value = GstRate + "%";
                        worksheet.Cells["I" + ExlRowNo].Value = GstAmt.ToString("F2");
                        worksheet.Cells["K" + ExlRowNo].Value = GstAmt.ToString("F2");
                    }
                    DuplicateRowBelowOnlyStyle(worksheet, ExlRowNo);
                    ExlRowNo++;
                }

                worksheet.Cells["A" + ExlRowNo].Value = "Total";
                worksheet.Cells["A" + ExlRowNo].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                if (InvTaxType == 0) //sgst cgst
                {
                    worksheet.Cells["E" + ExlRowNo].Value = TotalTaxableVal.ToString("F2");
                    worksheet.Cells["G" + ExlRowNo].Value = (TotalGstAmt / 2).ToString("F2");
                    worksheet.Cells["I" + ExlRowNo].Value = (TotalGstAmt / 2).ToString("F2");
                    worksheet.Cells["K" + ExlRowNo].Value = TotalGstAmt.ToString("F2");
                }
                else //igst
                {
                    worksheet.Cells["G" + ExlRowNo].Value = TotalTaxableVal.ToString("F2");
                    worksheet.Cells["I" + ExlRowNo].Value = TotalGstAmt.ToString("F2");
                    worksheet.Cells["K" + ExlRowNo].Value = TotalGstAmt.ToString("F2");
                }
                worksheet.Cells["D" + (ExlRowNo + 1)].Value = "INR " + NumToWordsConverter.AmountToWordsIndianFormat(TotalGstAmt);
                #endregion
                package.SaveAs(OutputFilePath);

                ConvertExcelToPdf(OutputFilePath, Path.GetDirectoryName(OutputFilePath) ?? "");
                return File.Exists(OutputFilePath.Replace(".xlsx", ".pdf")) ? File.ReadAllBytes(OutputFilePath.Replace(".xlsx", ".pdf")) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        public void DuplicateRowBelowOnlyStyle(ExcelWorksheet worksheet, int sourceRow)
        {
            try
            {
                int targetRow = sourceRow + 1;

                // Step 1: Insert a new empty row below
                worksheet.InsertRow(targetRow, 1);

                // Step 2: Copy each cell's value, formula, and style
                for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                {
                    var sourceCell = worksheet.Cells[sourceRow, col];
                    var targetCell = worksheet.Cells[targetRow, col];

                    // Copy formula or value
                    if (!string.IsNullOrWhiteSpace(sourceCell.Formula))
                    {
                        targetCell.Formula = sourceCell.Formula;
                    }
                    //else
                    //{
                    //    targetCell.Value = sourceCell.Value;
                    //}

                    // Copy style
                    targetCell.StyleID = sourceCell.StyleID;
                    //targetCell.Clear();
                }

                // Step 3: Copy row height
                worksheet.Row(targetRow).Height = worksheet.Row(sourceRow).Height;

                // Step 4: Copy any merged cells within the row
                foreach (var range in worksheet.MergedCells.ToList()) // ToList to avoid collection modification error
                {
                    var cell = worksheet.Cells[range];
                    if (cell.Start.Row == sourceRow && cell.End.Row == sourceRow)
                    {
                        int startCol = cell.Start.Column;
                        int endCol = cell.End.Column;

                        string newMergeRange = ExcelCellBase.GetAddress(targetRow, startCol, targetRow, endCol);
                        worksheet.Cells[newMergeRange].Merge = true;
                    }
                }
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
                    //Arguments = $"--headless --convert-to pdf \"{inputPath}\" --outdir \"{outputFolder}\"",
                    Arguments = $"--headless --convert-to pdf:\"calc_pdf_Export:ExportBookmarks=false\" \"{inputPath}\" --outdir \"{outputFolder}\"",
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
