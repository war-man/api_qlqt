using Api.ManagerGift.DTO;
using Api.ManagerGift.Entities;
using Api.ManagerGift.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;

namespace Api.ManagerGift.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TangQuaKhachHangController : ControllerBase
    {
        readonly private TangQuaKhachHangService _TangQuaKhachHangService = new TangQuaKhachHangService();

        [HttpGet("{accNo}/{phanhe}")]
        public IActionResult SearchAcc(string accNo, string phanhe)
        {
            try
            {
                var webClient = new WebClient();
                var url = string.Format("http://smartbankservice.cbbank.vn/serviceLab/Get_Customer_InfoNew_2019_LAB?accNo={0}&type={1}", accNo, phanhe);
                webClient.Encoding = Encoding.UTF8;
                var dataJson = webClient.DownloadString(url);
                var data = _TangQuaKhachHangService.Check(accNo, dataJson, phanhe, HttpContext.User);

                return Ok(data);
            }
            catch (System.Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        [HttpPost("{giftId}/{promotionId}/{soluong}")]
        public IActionResult TangQuaKhachHang([FromBody] CustomerDTO obj, string giftId, string promotionId, int soluong)
            => Ok(_TangQuaKhachHangService.TangQuaKhachHang(obj, giftId, promotionId, soluong, HttpContext.User));

        [HttpGet("{phanhe}/{acctno}/{promotion}")]
        public IActionResult LstTangQua(string phanhe, string acctno, string promotion)
            => Ok(_TangQuaKhachHangService.LstKhachHangNhanQua(phanhe, acctno, promotion));

        [HttpGet("Approve/{id}/{param}/{idGift}/{idPromotion}/{numGift}")]
        public IActionResult Approve(string id, string param, string idGift, string idPromotion, int numGift)
            => Ok(_TangQuaKhachHangService.Approve(id, param, idGift, idPromotion, numGift, HttpContext.User));

        [HttpGet("{id}")]
        public IActionResult DetailTangQua(string id)
            => Ok(_TangQuaKhachHangService.DetailTangQua(id));
    }
}