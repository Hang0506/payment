using FRTTMO.PaymentCore.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Repositories
{
    public interface IVoucherRepository
    {
        Task<List<Voucher>> GetByTransactionId(Guid transId);
        Task<Voucher> InsertVoucher(Voucher voucher);
        Task<List<Voucher>> GetByTransactionIds(List<Guid> transIds);
    }
}
