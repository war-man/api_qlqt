using Api.ManagerGift.DTO;
using Api.ManagerGift.Entities;
using Api.ManagerGift.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Api.ManagerGift.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PhanBoQuaTangController : ControllerBase
    {
        private readonly PhanBoQuaTangService _PhanBoQuaTangService = new PhanBoQuaTangService();

        [HttpGet("{pageNo}/{pageSize}/{organizationId}/{promotionId}")]
        public IActionResult Get(int pageNo,int pageSize, string organizationId, string promotionId)
        {
            return Ok(_PhanBoQuaTangService.Get(pageNo, pageSize, organizationId, promotionId, HttpContext.User));
        }

        [HttpPost("{flag}/{PromotionId}")]
        public IActionResult InitPhanBoQuaTang([FromBody] List<PhanBoQuaTang> obj, string flag, string PromotionId)
        {
            return Ok(_PhanBoQuaTangService.InitPhanBoQuaTang(obj, HttpContext.User, flag, PromotionId));
        }

        [HttpGet("DetailPhanBo/{TranferId}")]//{flagDieuChuyen}")]
        public IActionResult DetailPhanBoQuaTang(string TranferId)//string flagDieuChuyen)
        {
            //var _flagDieuChuyen = new Guid(flagDieuChuyen);
            var _tranferId = new Guid(TranferId);
            return Ok(_PhanBoQuaTangService.DetailPhanBoQuaTang(HttpContext.User,_tranferId)); //_flagDieuChuyen));
        }
        [HttpPost("Update/{TranferId}")]
        public IActionResult UpdatePhanBoQuaTang([FromBody] List<TransferDetail> obj,string TranferId)
        {
            return Ok(_PhanBoQuaTangService.UpdatePhanBoQuaTang(obj, HttpContext.User, TranferId));
        }
        [HttpGet("GetBranch/{flagDieuChuyen}/{idGift}")]
        public IActionResult GetBranch(string flagDieuChuyen, string idGift)
        {
            var _flagDieuChuyen = new Guid(flagDieuChuyen);
            var _idGift = new Guid(idGift);
            return Ok(_PhanBoQuaTangService.GetBranch(_flagDieuChuyen, _idGift));
        }

        [HttpPost("Duyet/{flagDieuChuyen}/{flag}/{id}")]
        public IActionResult Duyet([FromBody] ParamDTO obj,string flagDieuChuyen, string flag, string id)
        {
            var _flagDieuChuyen = new Guid(flagDieuChuyen);
            var _id = new Guid(id);
            return Ok(_PhanBoQuaTangService.Duyet(_flagDieuChuyen, flag, _id, obj.Comment, HttpContext.User));
        }
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            return Ok(_PhanBoQuaTangService.Delete(HttpContext.User, id));
        }
        // ================ Hoàn phân bổ quà tặng ==================//
        [HttpPost("HoanPhanBo/{flag}/{PromotionId}")]
        public IActionResult HoanPhanBo_LuuHoacGuiDuyet([FromBody] List<PhanBoQuaTang> obj, string flag, string PromotionId)
        {
            var _promotionId = new Guid(PromotionId);
            return Ok(_PhanBoQuaTangService.HoanPhanBo_LuuHoacGuiDuyet(obj, flag, _promotionId, HttpContext.User));
        }
        [HttpPost("UpdateHPB/{TranferId}")]
        public IActionResult UpdateHoanPhanBoQuaTang([FromBody] List<TransferDetail> obj, string TranferId)
        {
            return Ok(_PhanBoQuaTangService.UpdateHoanPhanBoQuaTang(obj, HttpContext.User, TranferId));
        }
        [HttpPost("HoanPhanBoDuyet/{flagDieuChuyen}/{flag}")]
        public IActionResult HoanPhanBo_Duyet([FromBody] ParamDTO obj, string flagDieuChuyen, string flag)
        {
            var _flagDieuChuyen = new Guid(flagDieuChuyen);
            return Ok(_PhanBoQuaTangService.HoanPhanBo_Duyet(_flagDieuChuyen, flag, obj.Comment, HttpContext.User));
        }
        [HttpPost("GuiDuyetHPB/{flag}/{id}")]
        public IActionResult GuiDuyet([FromBody] List<TransferDetail> obj,string flag, string id)
        {
            var _id = new Guid(id);
            return Ok(_PhanBoQuaTangService.HoanPhanBo_GuiDuyet(obj,flag, _id, HttpContext.User));
        }
    }
}