using FRTTMO.PaymentCore.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Repositories
{
    public class PaymentRequestRepository : ITransientDependency, IPaymentRequestRepository
    {
        private readonly IRepository<PaymentRequest, Guid> _repository;

        public PaymentRequestRepository(IRepository<PaymentRequest, Guid> repository)
        {
            _repository = repository;
        }

        public async Task<PaymentRequest> GetByOrderCode(string orderCode, Guid? orderReturnId = null)
        {
            //không được sử dụng nữa,v1
            return await _repository.FirstOrDefaultAsync
                (pay => pay.OrderCode == orderCode && pay.OrderReturnId == orderReturnId);
        }
        public async Task<PaymentRequest> InsertObj(PaymentRequest inItem)
        {
            inItem.CreatedDate = DateTime.Now;
            inItem = await _repository.InsertAsync(inItem, true);
            inItem.PaymentRequestCode = inItem.Id.ToString().Substring(0, 6).ToUpper() + inItem.PaymentRequestCode;
            var outItem = await _repository.UpdateAsync(inItem, true);
            return outItem;
        }
        public async Task<PaymentRequest> UpdateObj(PaymentRequest inItem)
        {
            inItem.ModifiedDate = DateTime.Now;
            inItem = await _repository.UpdateAsync(inItem, true);
            return inItem;
        }
        public async Task<bool> CheckExists(Guid guid) => await _repository.AnyAsync(c => c.Id == guid);
        public async Task<bool> CheckExists(string paymentRequestCode, DateTime? paymentRequestDate)
        {
            if (!paymentRequestDate.HasValue)
                paymentRequestDate = PaymentCoreUtilities.GetDate(paymentRequestCode);
            return await _repository.AnyAsync(c => c.PaymentRequestDate == paymentRequestDate && c.PaymentRequestCode == paymentRequestCode );
        }

        public async Task<PaymentRequest> GetById(Guid guid)
        {
            var rt = await _repository.FirstOrDefaultAsync(c => c.Id == guid);
            return rt;
        }

        public async Task<PaymentRequest> GetByPaymentRequestCode(string paymentRequestCode, DateTime? paymentRequestDate)
        {
            if (!paymentRequestDate.HasValue)
                paymentRequestDate = PaymentCoreUtilities.GetDate(paymentRequestCode);
            return await _repository.FirstOrDefaultAsync(pay => pay.PaymentRequestCode == paymentRequestCode && pay.PaymentRequestDate == paymentRequestDate);
        }

        public async Task<List<PaymentRequest>> GetListByOrderCode(string orderCode)
        {
            //không được sử dụng nữa,v1
            return await _repository.GetListAsync(pay => pay.OrderCode == orderCode);
        }

        public async Task<PaymentRequest> Update(PaymentRequest paymentRequest)
        {
            paymentRequest.ModifiedDate = DateTime.Now;
            paymentRequest = await _repository.UpdateAsync(paymentRequest, true);
            return paymentRequest;
        }
        public async Task<bool> CheckExistsComplete(string orderCode, EmPaymentRequestType typePayment)
        {
            //không được sử dụng nữa,v1
            return await _repository.AnyAsync(c => c.OrderCode.Equals(orderCode) && c.Status.Equals(EnmPaymentRequestStatus.Complete) && c.TypePayment.Equals(typePayment));
        }

        public async Task<PaymentRequest> GetByOrderReturnId(string orderCode, Guid orderReturnId)
        {
            //không được sử dụng nữa,v1
            return await _repository.FirstOrDefaultAsync(c => c.OrderCode.Equals(orderCode) && c.OrderReturnId.Equals(orderReturnId));
        }
        public async Task<PaymentRequest> GetToTalBill(string orderCode, EnmPaymentRequestStatus status)
        {
            //không được sử dụng nữa,v1
            return await _repository
                .FirstOrDefaultAsync(pay => pay.OrderCode == orderCode && pay.Status.Equals(status));
        }
        public async Task<List<PaymentRequest>> GetListOfPaymentCode(string paymentCode, EmPaymentRequestType type)
        {
            return await _repository.GetListAsync
                (
                    pr => 
                    pr.PaymentCode == paymentCode &&
                    pr.Status == EnmPaymentRequestStatus.Complete &&
                    pr.TypePayment == type
                );
        }
        public async Task<List<PaymentRequest>> GetListOfCode(string paymentCode)
        {
            return await _repository.GetListAsync
                (
                    pr =>
                    pr.PaymentCode == paymentCode &&
                    pr.TypePayment == EmPaymentRequestType.PaymentCoreRequest
                );
        }
        public async Task<List<PaymentRequest>> UpdateManyAsync(List<PaymentRequest> paymentRequest)
        {
            var dateTimeNow = DateTime.Now;
            paymentRequest.ForEach(x => x.ModifiedDate = dateTimeNow);
            await _repository.UpdateManyAsync(paymentRequest, true);
            return paymentRequest;
        }
        public async Task<PaymentRequest> GetByPaymentCode(string paymentCode)
        {
            return await _repository.FirstOrDefaultAsync(pay => pay.PaymentCode == paymentCode );
        }
        public async Task<List<PaymentRequest>> GetListByPaymentCode(string paymentCode)
        {
            return await _repository.GetListAsync(pay => pay.PaymentCode == paymentCode);
        }
    }
}
