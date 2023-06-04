using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace FRTTMO.PaymentCore.Dto
{
    public class RedisItemDto : EntityDto<Guid>
    {
        public string PaymentCode { get; set; }
    }
}
