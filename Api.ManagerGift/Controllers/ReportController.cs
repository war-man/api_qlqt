﻿using Api.ManagerGift.DTO;
using Api.ManagerGift.Services;
using Api.ManagerGift.Sessions;
using Aspose.Cells;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

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

        [HttpGet("GetDataTranfer/{pageNo}/{pageSize}/{productId}/{idPromotion}/{fromDate}/{toDate}")]
        public IActionResult GetDataTranfer(int pageNo, int pageSize, string productId, string idPromotion, string fromDate, string toDate)
        {
            var data = _reportService.GetDataReport(HttpContext.User, productId, idPromotion, fromDate, toDate);
            int total = data.ToList().Count();
            return Ok(new { List = data.ToList().Skip((pageNo - 1) * pageSize).Take(pageSize).ToList(), TotalPage = total % pageSize == 0 ? total / pageSize : total / pageSize + 1 });
        }
        [HttpGet("ExportTranfer/{productId}/{idPromotion}/{fromDate}/{toDate}")]
        public string GetExportTranfer(string productId, string idPromotion, string fromDate, string toDate)
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
                var data = _reportService.GetDataReport(HttpContext.User, productId, idPromotion, fromDate, toDate);
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
                var data = _reportService.GetDataReportInventory(HttpContext.User, productId, idPromotion, toDate);
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

        [HttpGet("GetInventory/{pageNo}/{pageSize}/{productId}/{idPromotion}/{toDate}")]
        public IActionResult GetInventory(int pageNo, int pageSize, string productId, string idPromotion, string toDate)
        {

            var data = _reportService.GetDataReportInventory(HttpContext.User, productId, idPromotion, toDate);
            int total = data.ToList().Count();
            return Ok(new { List = data.ToList().Skip((pageNo - 1) * pageSize).Take(pageSize).ToList(), TotalPage = total % pageSize == 0 ? total / pageSize : total / pageSize + 1 });
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
            newRow["Price"] = Convert.ToDecimal(p.Price).ToString("N0");
            newRow["TotalPrice"] = (p.Amount * decimal.Parse(p.Price)).ToString("N0");
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
            newRow["Price"] = Convert.ToDecimal(p.Price).ToString("N0");
            newRow["AmountAttribution"] = p.AmountAttribution;
            newRow["AmountUse"] = p.AmountUse;
            newRow["AmountInventory"] = p.AmountInventory;
            newRow["DepartmentName"] = p.DepartmentName;
            newRow["Note"] = p.Note;

            tbl.Rows.Add(newRow);
        }

        
        #endregion

        [HttpGet("GetDataBC07/{pageNo}/{pageSize}/{productId}/{idPromotion}/{idGift}/{idBranch}/{idDepartment}/{fromDate}/{toDate}")]
        public IActionResult GetDataBC_07(int pageNo, int pageSize, string productId, string idPromotion, string idGift, string idBranch, string idDepartment, string fromDate, string toDate)
        {
            var data = _reportService.GetDataReport07(HttpContext.User, productId, idPromotion, idGift, idBranch, idDepartment, fromDate, toDate);
            int total = data.ToList().Count();
            return Ok(new { List = data.ToList().Skip((pageNo - 1) * pageSize).Take(pageSize).ToList(), TotalPage = total % pageSize == 0 ? total / pageSize : total / pageSize + 1 });
        }
        [HttpGet("ExportBC07/{productId}/{idPromotion}/{idGift}/{idBranch}/{idDepartment}/{fromDate}/{toDate}")]
        public string GetExportBC_07(string productId, string idPromotion, string idGift, string idBranch, string idDepartment, string fromDate, string toDate)
        {
            var folder = "/DownloadWordExcel/";
            try
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                var contentRootPathTemp = _hostingEnvironment.ContentRootPath + "\\TempExport\\";
                var contentRootPathExport = _hostingEnvironment.ContentRootPath + "\\DownloadWordExcel\\";

                var nameReport = string.Empty;
                var nameTitle = string.Empty;
                var nameExport = string.Empty;

                switch (productId.ToUpper())
                {
                    case "BC_07":
                        nameTitle = "BÁO CÁO CHI TIẾT QUÀ TẶNG CHƯƠNG TRÌNH KHUYẾN MẠI";
                        nameExport = "Báo cáo chi tiết chương trình khuyến mại";
                        nameReport = "Bao-cao-chi-tiet-qua-tang-chuong-trinh-khuyen-mai";
                        break;
                    case "BC_10":
                        nameTitle = "BÁO CÁO QUÀ TẶNG NGOÀI CHƯƠNG TRÌNH";
                        nameExport = "Báo cáo quà tặng ngoài chương trình";
                        nameReport = "Bao-cao-qua-tang-ngoai-chuong-trinh-khuyen-mai";
                        break;
                    case "BC_08":
                        nameTitle = "BÁO CÁO TỔNG HỢP CHƯƠNG TRÌNH KHUYẾN MẠI";
                        nameExport = "Báo cáo tổng hợp chương trình khuyến mại";
                        nameReport = "Bao-cao-tong-hop-chuong-trinh-khuyen-mai";
                        break;
                    default:
                        break;
                }
                var workbook = new Workbook(contentRootPathTemp + nameReport + ".xlsx");
                var data = _reportService.GetDataReport07(HttpContext.User, productId, idPromotion, idGift, idBranch, idDepartment, fromDate, toDate);
                string[] title = new string[] { nameTitle, "Từ ngày " + fromDate + ", Đến ngày " + toDate };

                DataTable outData;
                DataTable(data, productId.ToUpper(), out outData);
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
        [HttpGet("GetDataBC09/{pageNo}/{pageSize}/{productId}/{idPromotion}/{idGiftStore}/{idGiftUse}/{idBranch}/{idDepartment}/{fromDate}/{toDate}")]
        public IActionResult GetDataBC_09(int pageNo, int pageSize, string productId, string idPromotion, string idGiftStore, string idGiftUse, string idBranch, string idDepartment, string fromDate, string toDate)
        {
            var data = _reportService.GetDataReport09(HttpContext.User, productId, idPromotion, idGiftStore, idGiftUse, idBranch, idDepartment, fromDate, toDate);
            int total = data.ToList().Count();
            return Ok(new { List = data.ToList().Skip((pageNo - 1) * pageSize).Take(pageSize).ToList(), TotalPage = total % pageSize == 0 ? total / pageSize : total / pageSize + 1 });
        }
        [HttpGet("ExportBC09/{productId}/{idPromotion}/{idGiftStore}/{idGiftUse}/{idBranch}/{idDepartment}/{fromDate}/{toDate}")]
        public string GetExportBC_09(string productId, string idPromotion, string idGiftStore, string idGiftUse, string idBranch, string idDepartment, string fromDate, string toDate)
        {
            var folder = "/DownloadWordExcel/";
            try
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                var contentRootPathTemp = _hostingEnvironment.ContentRootPath + "\\TempExport\\";
                var contentRootPathExport = _hostingEnvironment.ContentRootPath + "\\DownloadWordExcel\\";

                var nameReport = string.Empty;
                var nameTitle = string.Empty;
                var nameExport = string.Empty;

                switch (productId.ToUpper())
                {
                    case "BC_09":
                        nameTitle = "BÁO CÁO THEO DÕI QUÀ TẶNG";
                        nameExport = "Báo cáo đối chiếu rà soát";
                        nameReport = "Bao-cao-doi-chieu-ra-soat";
                        break;
                    default:
                        break;
                }
                var workbook = new Workbook(contentRootPathTemp + nameReport + ".xlsx");
                var data = _reportService.GetDataReport09(HttpContext.User, productId, idPromotion, idGiftStore, idGiftUse, idBranch, idDepartment, fromDate, toDate);
                string[] title = new string[] { nameTitle, "Từ ngày " + fromDate + ", Đến ngày " + toDate };

                DataTable outData;
                DataTable(data, productId.ToUpper(), out outData);
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
        private static void DataTable(List<BC06_DTO> dataInput, string key, out DataTable data)
        {
            var tbl = new DataTable();
            tbl.Columns.Add(new DataColumn("STT", typeof(string)));
            if(key!= "BC_09")
                tbl.Columns.Add(new DataColumn("BranchName", typeof(string)));
            tbl.Columns.Add(new DataColumn("DepartmentName", typeof(string)));
            switch (key)
            {
                case "BC_07":
                    tbl.Columns.Add(new DataColumn("SoTK", typeof(string)));
                    tbl.Columns.Add(new DataColumn("CustomerName", typeof(string)));
                    tbl.Columns.Add(new DataColumn("CIF", typeof(string)));
                    tbl.Columns.Add(new DataColumn("KyHan", typeof(string)));
                    tbl.Columns.Add(new DataColumn("SoDu", typeof(string)));
                    tbl.Columns.Add(new DataColumn("NgayGui", typeof(string)));
                    tbl.Columns.Add(new DataColumn("GiftName", typeof(string)));
                    tbl.Columns.Add(new DataColumn("GiaTriQuaTang", typeof(string)));
                    break;
                case "BC_08":
                    tbl.Columns.Add(new DataColumn("GiftCode", typeof(string)));
                    tbl.Columns.Add(new DataColumn("GiftName", typeof(string)));
                    tbl.Columns.Add(new DataColumn("SoLuong", typeof(string)));
                    tbl.Columns.Add(new DataColumn("GiaTriQuaTang", typeof(string)));
                    tbl.Columns.Add(new DataColumn("SoDu", typeof(string)));
                    break;
                case "BC_09":
                    tbl.Columns.Add(new DataColumn("GiftCode", typeof(string)));
                    tbl.Columns.Add(new DataColumn("GiftName", typeof(string)));
                    tbl.Columns.Add(new DataColumn("GiaTri", typeof(string)));
                    tbl.Columns.Add(new DataColumn("SoLuongNhapKho", typeof(string)));
                    tbl.Columns.Add(new DataColumn("SoLuongSuDung", typeof(string)));
                    tbl.Columns.Add(new DataColumn("SoLuongCuoiKy", typeof(string)));
                    //tbl.Columns.Add(new DataColumn("ChenhLech", typeof(string)));
                    tbl.Columns.Add(new DataColumn("ThanhTien", typeof(string)));
                    break;
                case "BC_10":
                    tbl.Columns.Add(new DataColumn("GiftName", typeof(string)));
                    tbl.Columns.Add(new DataColumn("MucDichSuDung", typeof(string)));
                    tbl.Columns.Add(new DataColumn("GiaTriQuaTang", typeof(string)));
                    tbl.Columns.Add(new DataColumn("NgayXuatQT", typeof(string)));
                    break;
                default:
                    break;
            }
            tbl.Columns.Add(new DataColumn("Note", typeof(string)));

            var idx = 0;
            foreach (var hs in dataInput)
            {
                idx++;
                AddDataTable(tbl, key, hs, idx);
            }

            data = tbl;
        }

        private static void AddDataTable(DataTable tbl, string key, BC06_DTO p, int idx)
        {
            var newRow = tbl.NewRow();

            newRow["STT"] = idx;
            if (key != "BC_09")
                newRow["BranchName"] = p.BranchName;
            newRow["DepartmentName"] = p.DepartmentName;
            switch (key)
            {
                case "BC_07":
                    newRow["SoTK"] = p.SoTK;
                    newRow["CustomerName"] = p.CustomerName;
                    newRow["CIF"] = p.CIF;
                    newRow["KyHan"] = p.KyHan;
                    newRow["SoDu"] = p.SoDu.ToString("N0");
                    newRow["NgayGui"] = p.NgayGui;
                    newRow["GiftName"] = p.GiftName;
                    newRow["GiaTriQuaTang"] = p.GiaTriQuaTang.ToString("N0");
                    break;
                case "BC_08":
                    newRow["GiftCode"] = p.GiftCode;
                    newRow["GiftName"] = p.GiftName;
                    newRow["SoLuong"] = p.SoLuong;
                    newRow["GiaTriQuaTang"] = p.GiaTriQuaTang.ToString("N0");
                    newRow["SoDu"] = p.SoDu;
                    break;
                case "BC_09":
                    newRow["GiftCode"] = p.GiftCode;
                    newRow["GiftName"] = p.GiftName;
                    newRow["GiaTri"] = p.GiaTri.ToString("N0");
                    newRow["SoLuongNhapKho"] = p.GiaTriQuaTang;
                    newRow["SoLuongSuDung"] = p.SoLuongSuDung;
                    newRow["SoLuongCuoiKy"] = p.SoLuongCuoiKy;
                    //newRow["ChenhLech"] = p.SoDu;
                    newRow["ThanhTien"] = p.SoDu.ToString("N0");
                    break;
                case "BC_10":
                    newRow["GiftName"] = p.GiftName;
                    newRow["MucDichSuDung"] = "";
                    newRow["GiaTriQuaTang"] = p.GiaTriQuaTang.ToString("N0");
                    newRow["NgayXuatQT"] = p.NgayXuatQT;
                    break;
                default:
                    break;
            }
            newRow["Note"] = "";
            tbl.Rows.Add(newRow);
        }
    }
}