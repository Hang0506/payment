using FRTTMO.PaymentCore.Application.Contracts;
using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Entities;
using FRTTMO.PaymentCore.Exceptions;
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
    public class CashbackService : PaymentCoreAppService, ITransientDependency, ICashbackService
    {
        private readonly ILogger<CardService> _log;
        private readonly IAccountRepository _accountRepository;
        private readonly IPaymentRequestRepository _iPaymentRequestRepository;
        private readonly IInternalAppServiceCore _internalAppService;
        private readonly ITransactionService _transactionService;
        private readonly IPublishService<BaseETO> _iPublishService;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public CashbackService(IAccountRepository accountRepository, IPaymentRequestRepository iPaymentRequestRepository, ILogger<CardService> log,
            IInternalAppServiceCore internalAppService,
            ITransactionService transactionService, IPublishService<BaseETO> iPublishService, IUnitOfWorkManager unitOfWorkManager) : base()
        {
            _accountRepository = accountRepository;
            _iPaymentRequestRepository = iPaymentRequestRepository;
            _log = log;
            _internalAppService = internalAppService;
            _transactionService = transactionService;
            _iPublishService = iPublishService;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task<TransactionFullOutputDto> CreateCashbackTransaction(CashbackInputBaseDto cashbackDto)
        {
            OrderReturnResult orderReturn;
            try
            {
                orderReturn = await _internalAppService.InvokeInternalAPI_GetData<OrderReturnResult>(EnvironmentSetting.RemoteOMSService, $"/api/oms/order-return/{cashbackDto.OrderReturnId}");
                if (orderReturn == null)
                {
                    throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_ORDERRETURNID_PAYMENT_NOTFOUND).WithData("OrderReturnId", cashbackDto.OrderReturnId);
                }
            }
            catch (Exception)
            {
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_ORDERRETURNID_PAYMENT_NOTFOUND).WithData("OrderReturnId", cashbackDto.OrderReturnId);
            }


            var order = await _internalAppService.InvokeInternalAPI_GetData<OMS_OrderResult>(EnvironmentSetting.RemoteOMSService, $"/api/oms/order/{cashbackDto.OrderCode}");
            if (order == null)
            {
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_INFO_NOTFOUND).WithData("OrderCode", cashbackDto.OrderCode);
            }
            else if (order.OrderStatus == OMSOrderStatus.Cancelled)
            {
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_ORDER_CANCEL).WithData("OrderCode", cashbackDto.OrderCode);
            }
            else if (cashbackDto.Totalpayment > orderReturn.TotalPayment)
            {
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_MONNEY_PAYMENT_NOTMATCHORDER).WithData("OrderId", cashbackDto.OrderCode);
            }
            else if (!await _iPaymentRequestRepository.CheckExists(cashbackDto.Transaction.PaymentRequestId.Value))
            {
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_REQUEST_NOTFOUND).WithData("PaymentRequestId", cashbackDto.Transaction.PaymentRequestId.Value);
            }
            else if (cashbackDto.Transaction.TransactionTypeId != EnmTransactionType.CashBack)
            {
                throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_TRANSACTION_TYPE_INVALID).WithData("TransactionTypeName", EnmTransactionType.CashBack);
            }
            else
            {
                var account = await _accountRepository.GetById(cashbackDto.Transaction.AccountId.Value);
                if (account == null)
                {
                    throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_ACCOUNT_NOTFOUND).WithData("AccountId", cashbackDto.Transaction.AccountId.Value);
                }
                else if (cashbackDto.Totalpayment >= account.CurrentBalance)
                {
                    throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_MONNEY_PAYMENT_NOTENOUGH).WithData("OrderId", cashbackDto.OrderCode);
                }
                else
                {
                    using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true))
                    {
                        //insert transaction
                        try
                        {
                            var transactionInput = cashbackDto.Transaction;
                            transactionInput.TransactionTypeId = EnmTransactionType.CashBack;
                            transactionInput.Status = EnmTransactionStatus.Created;
                            transactionInput.Amount = cashbackDto.Totalpayment;
                            var transactionOutput = await _transactionService.InsertTransaction(transactionInput);
                            //update account
                            account.CurrentBalance = account.CurrentBalance - cashbackDto.Totalpayment;
                            await _accountRepository.Update(account);

                            //Bắn kafka
                            var cashbackEto = new CashbackCreatedEto();
                            var transactionEto = ObjectMapper.Map<TransactionFullOutputDto, TransactionFullOutputEto>(transactionOutput);
                            cashbackEto.Transaction = transactionEto;
                            cashbackDto.OrderCode = cashbackDto.OrderCode;
                            await _iPublishService.ProduceAsync(cashbackEto);
                            await unitOfWork.SaveChangesAsync();
                            return transactionOutput;
                        }
                        catch (Exception ex)
                        {
                            await unitOfWork.RollbackAsync();
                            _log.LogError($"{_CoreName}.CreateCashbackTransaction: {ex}| Request body: {JsonConvert.SerializeObject(cashbackDto)}");
                            throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                                .WithData("OrderCode", cashbackDto.OrderCode)
                                .WithData("Message", ex.Message);
                        }
                    }
                }
            }

        }

    }
}
