using Api.ManagerGift.DTO;
using Api.ManagerGift.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Api.ManagerGift.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PromotionsController : ControllerBase
    {
        readonly private PromotionsService promotionsService = new PromotionsService();

        /// <summary>
        /// get danh sach chuong trinh khuyen mai.
        /// </summary>
        /// <param name="pageNo"></param>
        /// <param name="status"></param>
        /// <param name="namePromotion"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        [HttpGet("{pageNo}/{status}/{namePromotion}/{year}")]
        public IActionResult Get(int pageNo, string status, string namePromotion, int year) 
            => Ok(promotionsService.Get(HttpContext.User,pageNo, status, namePromotion, year));

        /// <summary>
        /// nhan vien gui lanh dao duyet: chuong trinh khuyen mai.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost("{flag}")]
        public IActionResult LuuHoacGuiDuyet([FromBody] PromotionsDTO obj, string flag) 
            => Ok(promotionsService.LuuHoacGuiDuyet(obj, HttpContext.User, flag));

        [HttpGet("GuiDuyet/{promotionId}")]
        public IActionResult GuiDuyet(string promotionId) 
            => Ok(promotionsService.GuiDuyet(new Guid(promotionId), HttpContext.User));

        [HttpGet("ActionLanhDao/{IdPromotion}/{flag}")]
        public IActionResult ActionLanhDao(string IdPromotion, string flag) 
            => Ok(promotionsService.ActionLanhDao(new Guid(IdPromotion), flag, HttpContext.User));

        /// <summary>
        /// get danh sach chuong trinh khuyen mai.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get() => Ok(promotionsService.Get());

        /// <summary>
        /// lay danh sach qua tang ung voi chuong trinh khuyen mai.
        /// </summary>
        /// <param name="Id">id chuong trinh khuyen mai</param>
        /// <returns></returns>
        [HttpGet("{Id}")]
        public IActionResult Get(string Id) => Ok(promotionsService.Get(new Guid(Id)));

        /// <summary>
        /// get chi tiet chuong trinh khuyen mai.
        /// </summary>
        /// <param name="IdPromotion"></param>
        /// <returns></returns>
        [HttpGet("Detail/{IdPromotion}")]
        public IActionResult GetPromotion(string IdPromotion) 
            => Ok(promotionsService.GetPromotion(new Guid(IdPromotion)));

        #region Nguyen Code
        /// <summary>
        /// nhan vien cap nhat lại khi trang thai la luu nhap
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost("UpdateCTKM/{flag}")]
        public IActionResult UpdateCTKM([FromBody] PromotionsDTO obj, string flag)
            => Ok(promotionsService.UpdatePromotion(obj, HttpContext.User, flag));

        [HttpGet("CheckMaCTKM/{codeCTKM}")]
        public IActionResult CheckMaCTKM(string codeCTKM) => Ok(promotionsService.CheckMaCTKM(codeCTKM));

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            return Ok(promotionsService.Delete(HttpContext.User,id));
        }
        #endregion
    }
}