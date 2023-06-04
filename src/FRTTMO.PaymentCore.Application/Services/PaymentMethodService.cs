using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Entities;
using FRTTMO.PaymentCore.Exceptions;
using FRTTMO.PaymentCore.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace FRTTMO.PaymentCore.Services
{
    public class PaymentMethodService : PaymentCoreAppService, ITransientDependency, IPaymentMethodService
    {
        private readonly IPaymentMethodRepository _paymentMethodRepository;
        private readonly IPaymentRequestRepository _paymentRequestRepository;
        private readonly ITransactionService _transactionService;
        private readonly IVendorRepository _vendorRepository;

        public PaymentMethodService(
            IPaymentMethodRepository paymentMethodRepository,
            IPaymentRequestRepository paymentRequestRepository,
            ITransactionService transactionService,
            IVendorRepository vendorRepository
        ) : base()
        {
            _paymentMethodRepository = paymentMethodRepository;
            _paymentRequestRepository = paymentRequestRepository;
            _transactionService = transactionService;
            _vendorRepository = vendorRepository;
        }

        public async Task<List<PaymentMethodDto>> GetListAsync()
        {
            var list = await _paymentMethodRepository.GetListAsync();
            var vendor = await _vendorRepository.GetListAsync();
            list.ForEach(c => c.Vendors = vendor.FindAll(v => v.PaymentMethodId.HasValue && v.PaymentMethodId == c.Id));
            return ObjectMapper.Map<List<PaymentMethod>, List<PaymentMethodDto>>(list);
        }

        public async Task<List<PaymentMethodDto>> GetListPaymentMethodByOrderCode(GetListPaymentMethodByOrderCodeInputDto inputDto)
        {
            var listPaymentRequest = await _paymentRequestRepository.GetListByOrderCode(inputDto.OrderCode);
            if (listPaymentRequest == null || listPaymentRequest.Count == 0)
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_INFO_NOTFOUND).WithData("OrderCode", inputDto.OrderCode);
            var getTransIsCollect = await _transactionService.GetByListPaymentRequestIds(listPaymentRequest.Select(x => x.Id).ToList());
            if (getTransIsCollect == null || getTransIsCollect.Count == 0)
                throw new BusinessException(PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION).WithData("Data", $"Transaction of Order {inputDto.OrderCode}");
            var listPaymentMethod = await _paymentMethodRepository.GetListByIdsAsync(getTransIsCollect.Select(x => (int)x.PaymentMethodId).Distinct().ToList());
            return ObjectMapper.Map<List<PaymentMethod>, List<PaymentMethodDto>>(listPaymentMethod);
        }
    }
}
