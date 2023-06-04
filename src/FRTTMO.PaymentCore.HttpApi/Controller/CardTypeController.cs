using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Controller
{
    [Route("api/PaymentCore/card-type")]
    [ApiVersion("1.0")]
    public class CardTypeController : PaymentCoreController<ICardTypeService>, ICardTypeService
    {
        public CardTypeController(ICardTypeService cardTypeService)
        {
            MainService = cardTypeService;
        }

        [HttpGet]
        public async Task<List<CardTypeFullOutputDto>> GetList() => await _mainService.GetList();
    }
}
