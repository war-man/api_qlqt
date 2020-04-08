using Api.ManagerGift.DTO;
using Api.ManagerGift.Services;
using Api.ManagerGift.Sessions;
using Aspose.Cells;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;

namespace Api.ManagerGift.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly ReportService _reportService = new ReportService();
        private readonly IHostingEnvironment _hostingEnvironment;
        public ReportController(IHostingEnvironment hostingEnvironment) => _hostingEnvironment = hostingEnvironment;

        [HttpGet("{productId}/{idPromotion}/{fromDate}/{toDate}")]
        public string Get(string productId, string idPromotion, string fromDate, string toDate)
        {
            var folder = "/DownloadWordExcel/";
            try
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                var contentRootPathTemp = _hostingEnvironment.ContentRootPath + "\\TempExport\\";
                var contentRootPathExport = _hostingEnvironment.ContentRootPath + "\\DownloadWordExcel\\";

                var nameReport = "Bao Cao Qua Tang";
                var nameTitle = string.Empty;
                var nameExport = string.Empty;

                switch (productId.ToUpper())
                {
                    case Constants.PARAM_XUAT_KHO:
                        nameTitle = "BÁO CÁO QUÀ TẶNG XUẤT KHO";
                        nameExport = "Báo cáo xuất kho";
                        break;
                    case Constants.PARAM_NHAP_KHO:
                        nameTitle = "BÁO CÁO QUÀ TẶNG NHẬP KHO";
                        nameExport = "Báo cáo nhập kho";
                        break;
                    case Constants.PARAM_DIEU_CHUYEN_NGANG:
                        nameTitle = "BÁO CÁO QUÀ TẶNG ĐIỀU CHUYỂN NGANG";
                        nameExport = "Báo cáo điều chuyển ngang";
                        break;
                    case Constants.PARAM_DIEU_CHUYEN_NOI_BO:
                        nameTitle = "BÁO CÁO QUÀ TẶNG ĐIỀU CHUYỂN NỘI BỘ";
                        nameExport = "Báo cáo điều chuyển nội bộ";
                        break;
                    default:
                        break;
                }
                var workbook = new Workbook(contentRootPathTemp + nameReport + ".xlsx");
                var data = _reportService.GetDataReport(productId, idPromotion, fromDate, toDate);
                string[] title = new string[] { nameTitle, "Từ ngày " + fromDate + ", Đến ngày " + toDate };

                DataTable outData;
                ConvertToDataTable(data, out outData);
                var worksheet = workbook.Worksheets[0];
                worksheet.Cells.ImportArray(title, 0, 0, true);
                worksheet.Cells.ImportDataTable(outData, false, "A5");
                var fileName = nameExport + " " + DateTime.Now.Year;
                workbook.Save(contentRootPathExport + fileName + ".xlsx", SaveFormat.Xlsx);
                return folder + fileName + ".xlsx";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return folder + "Error.xlsx";
            }
        }

        /// <summary>
        /// Báo cáo tồn kho quà tặng
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="idPromotion"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        [HttpGet("ReportInventory/{productId}/{idPromotion}/{toDate}")]
        public string ReportInventory(string productId, string idPromotion, string toDate)
        {
            var folder = "/DownloadWordExcel/";
            try
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                var contentRootPathTemp = _hostingEnvironment.ContentRootPath + "\\TempExport\\";
                var contentRootPathExport = _hostingEnvironment.ContentRootPath + "\\DownloadWordExcel\\";

                var nameReport = "Bao Cao Ton Kho";
                var nameTitle = string.Empty;
                var nameExport = string.Empty;

                switch (productId.ToUpper())
                {
                    case "BC_06":
                        nameTitle = "BÁO CÁO QUÀ TẶNG TỒN KHO";
                        nameExport = "Báo cáo tồn kho";
                        break;
                    default:
                        break;
                }
                var workbook = new Workbook(contentRootPathTemp + nameReport + ".xlsx");
                var data = _reportService.GetDataReportInventory(HttpContext.User,productId, idPromotion, toDate);
                string[] title = new string[] { nameTitle, " ngày " + toDate };

                DataTable outData;
                ConvertToDataTableStore(data, out outData);
                var worksheet = workbook.Worksheets[0];
                worksheet.Cells.ImportArray(title, 0, 0, true);
                worksheet.Cells.ImportDataTable(outData, false, "A5");
                
                var fileName = DateTime.Now.ToString("yyyyMMdd_hhmmss_") + nameExport;
                workbook.Save(contentRootPathExport + fileName + ".xlsx", SaveFormat.Xlsx);
                return folder + fileName + ".xlsx";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return folder + "Error.xlsx";
            }
        }

        [HttpGet]
        public IActionResult GetTonKho() => Ok(_reportService.TonKho(HttpContext.User));

        #region private
        private static void ConvertToDataTable(IEnumerable<BaoCaoQuaTangDTO> dataInput, out DataTable data)
        {
            var tbl = new DataTable();
            tbl.Columns.Add(new DataColumn("STT", typeof(string)));
            tbl.Columns.Add(new DataColumn("Name", typeof(string)));
            tbl.Columns.Add(new DataColumn("Code", typeof(string)));
            tbl.Columns.Add(new DataColumn("Amount", typeof(string)));
            tbl.Columns.Add(new DataColumn("UnitName", typeof(string)));
            tbl.Columns.Add(new DataColumn("Price", typeof(string)));
            tbl.Columns.Add(new DataColumn("TotalPrice", typeof(string)));
            tbl.Columns.Add(new DataColumn("CreatedDate", typeof(string)));
            tbl.Columns.Add(new DataColumn("TranferDepartment", typeof(string)));
            tbl.Columns.Add(new DataColumn("ReceivingDepartment", typeof(string)));
            tbl.Columns.Add(new DataColumn("Note", typeof(string)));

            var idx = 0;

            foreach (var hs in dataInput)
            {
                idx++;
                AddToDataTable(tbl, hs, idx);
            }

            data = tbl;
        }

        private static void AddToDataTable(DataTable tbl, BaoCaoQuaTangDTO p, int idx)
        {
            var newRow = tbl.NewRow();

            newRow["STT"] = idx;
            newRow["Name"] = p.Name;
            newRow["Code"] = p.Code;
            newRow["Amount"] = p.Amount;
            newRow["UnitName"] = p.UnitName;
            newRow["Price"] = p.Price;
            newRow["TotalPrice"] = (p.Amount * decimal.Parse(p.Price));
            newRow["CreatedDate"] = p.CreatedDate;
            newRow["TranferDepartment"] = p.TranferDepartment;
            newRow["ReceivingDepartment"] = p.ReceivingDepartment;
            newRow["Note"] = "";

            tbl.Rows.Add(newRow);
        }

        private static void ConvertToDataTableStore(IEnumerable<StoreDTO> dataInput, out DataTable data)
        {
            var tbl = new DataTable();
            tbl.Columns.Add(new DataColumn("STT", typeof(string)));
            tbl.Columns.Add(new DataColumn("GiftName", typeof(string)));
            tbl.Columns.Add(new DataColumn("GiftCode", typeof(string)));
            tbl.Columns.Add(new DataColumn("Price", typeof(string)));
            tbl.Columns.Add(new DataColumn("AmountAttribution", typeof(string)));
            tbl.Columns.Add(new DataColumn("AmountUse", typeof(string)));
            tbl.Columns.Add(new DataColumn("AmountInventory", typeof(string)));
            tbl.Columns.Add(new DataColumn("DepartmentName", typeof(string)));
            tbl.Columns.Add(new DataColumn("Note", typeof(string)));

            var idx = 0;

            foreach (var hs in dataInput)
            {
                idx++;
                AddToDataTableStore(tbl, hs, idx);
            }

            data = tbl;
        }
        private static void AddToDataTableStore(DataTable tbl, StoreDTO p, int idx)
        {
            var newRow = tbl.NewRow();

            newRow["STT"] = idx;
            newRow["GiftName"] = p.GiftName;
            newRow["GiftCode"] = p.GiftCode;
            newRow["Price"] = p.Price;
            newRow["AmountAttribution"] = p.AmountAttribution;
            newRow["AmountUse"] = p.AmountUse;
            newRow["AmountInventory"] = p.AmountInventory;
            newRow["DepartmentName"] = p.DepartmentName;
            newRow["Note"] = p.Note;

            tbl.Rows.Add(newRow);
        }
        #endregion
    }
}