using Api.ManagerGift.DTO;
using Api.ManagerGift.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Api.ManagerGift.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TransferController : ControllerBase
    {
        private readonly TransferService _transferService = new TransferService();
        private readonly TransferInputService _transerInputService = new TransferInputService();
        private readonly TransferOutputService _transferOutputService = new TransferOutputService();

        [HttpGet("{pageNo}/{pageSize}/{idProduct}/{maQuaTang}/{donViThucHien}/{donViChuyen}/{donViNhan}")]
        public IActionResult GetListTranfer(int pageNo, int pageSize, string idProduct, string maQuaTang, string donViThucHien, string donViChuyen, string donViNhan)
        {
            return Ok(_transferService.Get(pageNo,pageSize, HttpContext.User, idProduct, maQuaTang, donViThucHien, donViChuyen, donViNhan));
        }

        [HttpGet("TransDetail/{id}")]
        public IActionResult GetDetail(Guid id)
        {
            return Ok(_transferService.GetDetailTranfer(HttpContext.User, id));
        }

        [HttpPost("InitTransfer/{productId}/{flag}")]
        public IActionResult InitTransfer([FromBody] List<DataNhapKho> obj, string productId, string flag)
        {
            return Ok(_transferService.InitTransfer(obj, productId, HttpContext.User, flag));
        }
        [HttpPost("InitTransferUpdate/{productId}/{flag}/{Id}")]
        public IActionResult InitTransferUpdate([FromBody] List<DataNhapKho> obj, string productId, string flag, string Id)
        {
            return Ok(_transferService.InitTransferUpdate(obj, productId, HttpContext.User, flag,Id));
        }
        [HttpDelete("InitTransferDelete/{Id}")]
        public IActionResult InitTransferDelete(string Id)
        {
            return Ok(_transferService.InitTransferDelete(Guid.Parse(Id)));
        }
        [HttpPost("DieuChuyenNgang/{flag}/{fromOrganId}/{toOrganId}")]
        public IActionResult DieuChuyenNgang([FromBody] List<DataNhapKho> obj, string flag, string fromOrganId, string toOrganId)
        {
            return Ok(_transferService.InitDieuChuyenNgang(obj, flag, fromOrganId, toOrganId, HttpContext.User));
        }

        [HttpPost("DieuChuyenNoiBo/{flag}/{fromPromotionId}/{toPromotionId}")]
        public IActionResult DieuChuyenNoiBo([FromBody] List<DataNhapKho> obj, string flag, string fromPromotionId, string toPromotionId)
        {
            return Ok(_transferService.InitDieuChuyenNoiBo(obj, flag, fromPromotionId, toPromotionId, HttpContext.User));
        }
        [HttpPost("DieuChuyenNgangUpdate/{flag}/{fromOrganId}/{toOrganId}/{Id}")]
        public IActionResult DieuChuyenNgangUpdate([FromBody] List<DataNhapKho> obj, string flag, string fromOrganId, string toOrganId, string Id)
        {
            return Ok(_transferService.InitDieuChuyenNgangUpdate(obj, flag, fromOrganId, toOrganId, Id, HttpContext.User));
        }

        [HttpPost("DieuChuyenNoiBoUpdate/{flag}/{fromPromotionId}/{toPromotionId}/{Id}")]
        public IActionResult DieuChuyenNoiBoUpdate([FromBody] List<DataNhapKho> obj, string flag, string fromPromotionId, string toPromotionId, string Id)
        {
            return Ok(_transferService.InitDieuChuyenNoiBoUpdate(obj, flag, fromPromotionId, toPromotionId, Id, HttpContext.User));
        }
        //[HttpPost("InitTransfer")]
        //public IActionResult InitTransfer([FromBody] List<DataNhapKho> obj, string productId, string flag)
        //{
        //    return Ok(_transferService.InitTransfer(obj, productId, HttpContext.User, flag));
        //}

        // nhân viên lưu thông tin nhập kho, mà chưa gửi duyệt
        //[HttpPost("PostDraft")]
        //public IActionResult PostDraft([FromBody] TransferGiftDTO obj)
        //{
        //    return Ok(_transferService.PostDraft(obj, HttpContext.User));
        //}

        //[HttpPost("Browse")]
        //public IActionResult Browse([FromBody] TransferGiftDTO obj)
        //{
        //    return Ok(_transferService.Browse(obj, HttpContext.User));
        //}

        //[HttpPost("Refuse/{transferId}")]
        //public IActionResult Refuse(Guid transferId)
        //{
        //    return Ok(_transferService.Refuse(transferId, HttpContext.User));
        //}

        [HttpGet("NhanVienGuiDuyet/{idTranfer}/{productId}")]
        public IActionResult NhanVienGuiDuyet(string idTranfer, string productId)
        {
            var _idTranfer = new Guid(idTranfer);
            return Ok(_transferService.NhanVienGuiDuyet(_idTranfer, productId, HttpContext.User));
        }

        [HttpGet("LanhDaoDuyet/{idTranfer}/{productId}")]
        public IActionResult LanhDaoDuyet(string idTranfer, string productId)
        {
            var _idTranfer = new Guid(idTranfer);
            return Ok(_transferService.LanhDaoDuyet(_idTranfer, productId, HttpContext.User));
        }

        [HttpPost("LanhDaoTuChoiDuyet/{idTranfer}/{productId}")]
        public IActionResult LanhDaoTuChoiDuyet([FromBody] DataNhapKho obj ,string idTranfer, string productId)
        {
            var _idTranfer = new Guid(idTranfer);
            return Ok(_transferService.LanhDaoTuChoiDuyet(_idTranfer, obj.Note, productId, HttpContext.User));
        }

        [HttpGet("TranferHistory/{idTranfer}")]
        public IActionResult TranferHistory(string idTranfer) 
            => Ok(_transferService.TranferHistory(idTranfer));

        #region Nguyen
        [HttpGet("TranferHistory/{promotionId}")]
        public IActionResult TranferPromotion(string promotionId)
            => Ok(_transferService.TranferPromotion(promotionId));
        #endregion
    }
}