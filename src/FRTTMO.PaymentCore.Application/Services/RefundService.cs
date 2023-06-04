using FRTTMO.PaymentCore.Application.Contracts;
using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Kafka.Eto;
using FRTTMO.PaymentCore.Kafka.Interface;
using FRTTMO.PaymentCore.Repositories;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Services
{
    public class RefundService : PaymentCoreAppService, ITransientDependency, IRefundService
    {
        private readonly ILogger<RefundService> _log;
        private readonly ITransactionService _transactionService;
        private readonly IAccountService _accountService;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IAccountRepository _accountRepository;
        private readonly IPublishService<BaseETO> _iPublishService;

        public RefundService(ILogger<RefundService> log,
            ITransactionService transactionService,
            IAccountService accountService,
            IUnitOfWorkManager unitOfWorkManager,
            IAccountRepository accountRepository,
            IPublishService<BaseETO> iPublishService) : base()
        {
            _log = log;
            _transactionService = transactionService;
            _accountService = accountService;
            _unitOfWorkManager = unitOfWorkManager;
            _accountRepository = accountRepository;
            _iPublishService = iPublishService;
        }

        public async Task<RefundFullOutputDto> CreateTransaction(RefundDto refundDto)
        {
            // Bước 1: Check thông tin
            // 1.1 Check thông tin OrderID: Get OMS.OrderInfo by PaymentCode
            //_log.LogInformation("runing refund");
            //try
            //{
            //    var omsOrder = await _internalAppService.OMSGetOrderByCode(refundDto.PaymentCode);
            //    if (omsOrder.PaymentCode == null || omsOrder.PaymentCode.Length < 20)
            //    {
            //        throw new BusinessException(PaymentCoreErrorCodes.ERROR_ORDERCODE_NOTFOUND).WithData("Data", false);
            //    }
            //}
            //catch (BusinessException)
            //{
            //    throw new BusinessException(PaymentCoreErrorCodes.ERROR_ORDERCODE_NOTFOUND).WithData("Data", false);
            //}
            //catch (Exception)
            //{
            //    throw new BusinessException(PaymentCoreErrorCodes.ERROR_ORDERCODE_NOTFOUND).WithData("Data", false);
            //}
            if (string.IsNullOrEmpty(refundDto.OrderCode) || refundDto.OrderCode.Length < 20)
            {
                throw new BusinessException(PaymentCoreErrorCodes.ERROR_ORDERCODE_NOTFOUND).WithData("Data", false);
            }
            //_log.LogInformation("runing refund 1");
            // 1.2 Check CustomerID có tồn tại trong bảng dbo.Account không
            try
            {
                var customerOutput = await _accountService.GetBalanceByCustomerId(refundDto.CustomerId);
                if (customerOutput == null || customerOutput.CustomerId.ToString().Length < 20)
                {
                    throw new BusinessException(PaymentCoreErrorCodes.ERROR_CUSTOMER_NOTFOUND).WithData("Data", false);
                }
            }
            catch (BusinessException)
            {
                throw new BusinessException(PaymentCoreErrorCodes.ERROR_CUSTOMER_NOTFOUND).WithData("Data", false);
            }
            catch (Exception)
            {
                throw new BusinessException(PaymentCoreErrorCodes.ERROR_CUSTOMER_NOTFOUND).WithData("Data", false);
            }

            // Bước 2:
            // 2.1 Insert thông tin hoàn tiền vào bảng Transtion và requestID tương ứng, +transactionTypeId(= 3), paymentMethodId(= 1)
            // 2.2 Update vào table acount : current_balance = current_balance + amount
            //_log.LogInformation("runing refund2");
            //Insert  transaction
            using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true))
            {
                try
                {
                    // 2.1 và 2.2
                    var trans = await InsertTransactionWithDetail(refundDto);

                    // 3 Bắn kafka create lc.payment.transaction.created , lc.payment.paymentRequest.created ( bắn 2 topic)
                    // Topic 1: lc.payment.transaction.created
                    var eto_1 = new RefundCreatedETO_1
                    {
                        OrderCode = trans.OrderCode,
                        Data = trans
                    };
                    await _iPublishService.ProduceAsync(eto_1);

                    // Topic 2: lc.payment.paymentRequest.created
                    var eto_2 = new RefundCreatedETO_2
                    {
                        OrderCode = trans.OrderCode,
                        Data = trans
                    };
                    await _iPublishService.ProduceAsync(eto_2);

                    //Save
                    await unitOfWork.SaveChangesAsync();
                    return trans;
                }
                catch (BusinessException bex)
                {
                    await unitOfWork.RollbackAsync();
                    _log.LogError($"{_CoreName}.CreateTransaction: {bex}| Request body: {JsonConvert.SerializeObject(refundDto)}");
                    throw bex;
                }
                catch (Exception ex)
                {
                    await unitOfWork.RollbackAsync();
                    _log.LogError($"{_CoreName}.CreateTransaction: {ex}| Request body: {JsonConvert.SerializeObject(refundDto)}");
                    throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                        .WithData("OrderCode", refundDto.OrderCode)
                        .WithData("Message", ex.Message);
                }
            }
            //_log.LogInformation("runing refund3");
        }

        private async Task<RefundFullOutputDto> InsertTransactionWithDetail(RefundDto refundDto)
        {
            var output = new RefundFullOutputDto();

            // 2.1 Insert thông tin hoàn tiền vào bảng Transtion và requestID tương ứng

            refundDto.TransactionTypeId = Convert.ToInt32(EnmTransactionType.Refund);
            refundDto.PaymentMethodId = Convert.ToInt32(EnmPaymentMethod.Cash);

            var transactionInput = ObjectMapper.Map<RefundDto, InsertTransactionInputDto>(refundDto);
            var transactionOutput = await _transactionService.InsertTransaction(transactionInput);

            // Map kết quả trả về
            output = ObjectMapper.Map<TransactionFullOutputDto, RefundFullOutputDto>(transactionOutput);
            output.OrderCode = refundDto.OrderCode;

            // 2.2 Update vào table acount

            if (refundDto.Amount.HasValue)
            {
                var acc = await _accountRepository.GetByCustomerId(refundDto.CustomerId);
                acc.CurrentBalance = (acc.CurrentBalance ?? 0m) + refundDto.Amount.Value;
                await _accountRepository.Update(acc);
            }

            return output;
        }
    }
}
