using FRTTMO.PaymentCore.Application.Contracts;
using FRTTMO.PaymentCore.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Services
{
    public interface IDebitService : IPaymentCoreAppServiceBase
    {
        Task<DebitFullOutputDto> InsertAsync(DebitDto input);
        Task<List<DebitFullOutputDto>> GetByTransactionIds(List<Guid> transIds);
        Task<List<DebitFullOutputDto>> GetByTransactionId(Guid transId);
    }
}