using FRTTMO.PaymentCore.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Repositories
{
    public interface IPaymentRequestRepository
    {
        Task<PaymentRequest> GetByOrderCode(string orderCode, Guid? orderReturnId = null);
        Task<PaymentRequest> InsertObj(PaymentRequest inItem);
        Task<PaymentRequest> UpdateObj(PaymentRequest inItem);
        Task<bool> CheckExists(Guid guid);
        Task<bool> CheckExists(string paymentRequestCode, DateTime? paymentRequestDate);
        Task<PaymentRequest> GetById(Guid guid);
        Task<PaymentRequest> GetByPaymentRequestCode(string paymentRequestCode, DateTime? paymentRequestDate);
        Task<List<PaymentRequest>> GetListByOrderCode(string orderCode);
        Task<PaymentRequest> Update(PaymentRequest paymentRequest);
        Task<bool> CheckExistsComplete(string orderCode, EmPaymentRequestType typePayment);
        Task<PaymentRequest> GetByOrderReturnId(string orderCode, Guid orderReturnId);
        Task<PaymentRequest> GetToTalBill(string orderCode, EnmPaymentRequestStatus status);
        Task<List<PaymentRequest>> GetListOfPaymentCode(string paymentCode, EmPaymentRequestType type);
        Task<List<PaymentRequest>> GetListOfCode(string paymentCode);
        Task<List<PaymentRequest>> UpdateManyAsync(List<PaymentRequest> paymentRequest);
        Task<PaymentRequest> GetByPaymentCode(string paymentCode);
        Task<List<PaymentRequest>> GetListByPaymentCode(string paymentCode);
    }
}
