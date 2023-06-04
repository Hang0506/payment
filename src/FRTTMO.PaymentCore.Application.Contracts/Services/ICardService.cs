using FRTTMO.PaymentCore.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace FRTTMO.PaymentCore.Services
{
    public interface ICardService: IApplicationService
    {
        Task<List<CardFullOutputDto>> GetByTransactionId(Guid transId);
        Task<List<CardFullOutputDto>> GetByTransactionIds(List<Guid> transIds);
        Task<CardFullOutputDto> InsertCard(CardInputDto cardDto);
    }
}
