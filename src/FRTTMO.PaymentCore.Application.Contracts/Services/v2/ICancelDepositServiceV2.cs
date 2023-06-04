using FRTTMO.PaymentCore.Application.Contracts;
using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Dto.v2;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Services.v2
{
    public interface ICancelDepositServiceV2 : IPaymentCoreAppServiceBase
    {
        Task<CreatePaymentTransactionOutputDtoV2> CreateTransactiontransfer(TransactionCancelDepositTransferV2 request);
    }
}
