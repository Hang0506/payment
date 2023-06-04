using FRTTMO.PaymentCore.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Services
{
    public interface IVoucherService
    {
        Task<List<VoucherFullOutputDto>> GetByTransactionId(Guid transId);
        Task<VoucherFullOutputDto> InsertVoucher(VoucherInputDto voucher);
        Task<List<VoucherFullOutputDto>> GetByTransactionIds(List<Guid> transIds);
    }
}
