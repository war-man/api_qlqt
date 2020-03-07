using Api.ManagerGift.DTO;
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

        [HttpGet("{pageNo}/{organizationId}/{promotionId}")]
        public IActionResult Get(int pageNo, string organizationId, string promotionId)
        {
            return Ok(_PhanBoQuaTangService.Get(pageNo, organizationId, promotionId, HttpContext.User));
        }

        [HttpPost("{flag}/{PromotionId}")]
        public IActionResult InitPhanBoQuaTang([FromBody] List<PhanBoQuaTang> obj, string flag, string PromotionId)
        {
            return Ok(_PhanBoQuaTangService.InitPhanBoQuaTang(obj, HttpContext.User, flag, PromotionId));
        }

        [HttpGet("DetailPhanBo/{flagDieuChuyen}")]
        public IActionResult DetailPhanBoQuaTang(string flagDieuChuyen)
        {
            var _flagDieuChuyen = new Guid(flagDieuChuyen);
            return Ok(_PhanBoQuaTangService.DetailPhanBoQuaTang(_flagDieuChuyen));
        }

        [HttpGet("GetBranch/{flagDieuChuyen}/{idGift}")]
        public IActionResult GetBranch(string flagDieuChuyen, string idGift)
        {
            var _flagDieuChuyen = new Guid(flagDieuChuyen);
            var _idGift = new Guid(idGift);
            return Ok(_PhanBoQuaTangService.GetBranch(_flagDieuChuyen, _idGift));
        }

        [HttpGet("Duyet/{flagDieuChuyen}/{flag}")]
        public IActionResult Duyet(string flagDieuChuyen, string flag)
        {
            var _flagDieuChuyen = new Guid(flagDieuChuyen);
            return Ok(_PhanBoQuaTangService.Duyet(_flagDieuChuyen, flag, HttpContext.User));
        }

        // ================ Hoàn phân bổ quà tặng ==================//
        [HttpPost("HoanPhanBo/{flag}/{PromotionId}")]
        public IActionResult HoanPhanBo_LuuHoacGuiDuyet([FromBody] List<PhanBoQuaTang> obj, string flag, string PromotionId)
        {
            var _promotionId = new Guid(PromotionId);
            return Ok(_PhanBoQuaTangService.HoanPhanBo_LuuHoacGuiDuyet(obj, flag, _promotionId, HttpContext.User));
        }

        [HttpGet("HoanPhanBo/{flagDieuChuyen}/{flag}")]
        public IActionResult HoanPhanBo_Duyet(string flagDieuChuyen, string flag)
        {
            var _flagDieuChuyen = new Guid(flagDieuChuyen);
            return Ok(_PhanBoQuaTangService.HoanPhanBo_Duyet(_flagDieuChuyen, flag, HttpContext.User));
        }
    }
}