using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Entities;
using FRTTMO.PaymentCore.Repositories;
using FRTTMO.PaymentCore.Services.v2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Services
{
    public class TransactionService : PaymentCoreAppService, ITransientDependency, ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICardService _cardService;
        private readonly ICODService _cODService;
        private readonly ITransferService _transferService;
        private readonly IVoucherService _voucherService;
        private readonly IEWalletDepositService _eWalletDepositService;
        private readonly IAccountRepository _accountRepository;
        private readonly IGenerateServiceV2 _generateServiceV2;
        private IDebitService _debitService;

        public TransactionService(
            ITransactionRepository transactionRepository,
            IEWalletDepositService eWalletDepositService,
            ICardService cardService,
            ICODService cODService,
            ITransferService transferService,
            IVoucherService voucherService,
            IAccountRepository accountRepository,
            IDebitService debitService,
            IGenerateServiceV2 generateServiceV2)
        {
            _transactionRepository = transactionRepository;
            _eWalletDepositService = eWalletDepositService;
            _cardService = cardService;
            _cODService = cODService;
            _transferService = transferService;
            _voucherService = voucherService;
            _accountRepository = accountRepository;
            _generateServiceV2 = generateServiceV2;
            _debitService = debitService;
        }

        public async Task<List<TransactionFullOutputDto>> GetByPaymentRequestInfo(GetByPaymentRequestInfoInput infoInput)
        {
            var trans = await _transactionRepository.GetByPaymentRequestInfo(infoInput.paymentRequestId, infoInput.transactionTypeIds, infoInput.paymentRequestDate);
            var rs = trans.Select(ObjectMapper.Map<Transaction, TransactionFullOutputDto>).ToList();
            return rs;
        }

        public async Task<List<TransactionFullOutputDto>> GetByPaymentRequestId(Guid paymentRequestId, DateTime? paymentRequestDate)
        {
            var trans = await _transactionRepository.GetByPaymentRequestId(paymentRequestId, paymentRequestDate);
            var rs = trans.Select(ObjectMapper.Map<Transaction, TransactionFullOutputDto>).ToList();
            return rs;
        }

        public async Task<decimal?> GetTotalDeposited(Guid paymentRequestId, DateTime? paymentRequestDate)
            => await _transactionRepository.GetTotalDeposited(paymentRequestId, paymentRequestDate);
        public async Task<TransactionFullOutputDto> InsertTransaction(InsertTransactionInputDto transactionInputDto)
        {
            var inputET = ObjectMapper.Map<InsertTransactionInputDto, Transaction>(transactionInputDto);
            if (inputET.PaymentRequestDate == null || inputET.PaymentRequestDate.Value == default)
                inputET.PaymentRequestDate = _generateServiceV2.GetPaymentRequestDate(inputET.PaymentRequestCode);
            var outputET = await _transactionRepository.InsertTransaction(inputET);
            return ObjectMapper.Map<Transaction, TransactionFullOutputDto>(outputET);
        }

        public async Task<DepositWalletOutputDto> InsertTransactionWithDetail(DepositCoresInputDto rechargeWalletInput, bool UpdateAccountAmount = false)
        {
            var output = new DepositWalletOutputDto();
            var transactionInput = rechargeWalletInput.Transaction;
            transactionInput.Status = EnmTransactionStatus.Created;
            var transactionOutput = await InsertTransaction(transactionInput);
            output.TransactionId = transactionOutput.Id;
            output.Transaction = transactionOutput;

            if (rechargeWalletInput.Transaction.PaymentMethodId == EnmPaymentMethod.Card)
            {
                output.Cards = new List<CardFullOutputDto> { };
                foreach (var chil in rechargeWalletInput.Cards)
                {
                    chil.TransactionId = output.TransactionId;
                    var cardOutput = await _cardService.InsertCard(chil);
                    output.Cards.Add(cardOutput);
                }
            }
            else if (rechargeWalletInput.Transaction.PaymentMethodId == EnmPaymentMethod.COD)
            {
                rechargeWalletInput.COD.TransactionId = output.TransactionId;
                output.COD = await _cODService.Insert(rechargeWalletInput.COD);
            }
            else if (rechargeWalletInput.Transaction.PaymentMethodId == EnmPaymentMethod.Transfer)
            {
                output.Transfers = new List<TransferFullOutputDto> { };
                foreach (var chil in rechargeWalletInput.Transfers)
                {
                    chil.TransactionId = output.TransactionId;
                    var transferOutput = await _transferService.Insert(chil);
                    output.Transfers.Add(transferOutput);
                }
            }
            else if (rechargeWalletInput.Transaction.PaymentMethodId == EnmPaymentMethod.Voucher)
            {
                output.Vouchers = new List<VoucherFullOutputDto>();
                foreach (var chil in rechargeWalletInput.Vouchers)
                {
                    chil.TransactionId = output.TransactionId;
                    var voucherOutput = await _voucherService.InsertVoucher(chil);
                    output.Vouchers.Add(voucherOutput);
                }
            }
            else if (rechargeWalletInput.Transaction.PaymentMethodId == EnmPaymentMethod.Wallet)
            {
                var walletInput = rechargeWalletInput.EWallet;
                walletInput.TransactionId = output.TransactionId;
                output.EWallet = await _eWalletDepositService.InsertEWalletDeposit(walletInput);
            }
            else if (
                rechargeWalletInput.Transaction.PaymentMethodId == EnmPaymentMethod.VNPayGateway
                || rechargeWalletInput.Transaction.PaymentMethodId == EnmPaymentMethod.AlepayGateway
                || rechargeWalletInput.Transaction.PaymentMethodId == EnmPaymentMethod.MocaEWallet
                || rechargeWalletInput.Transaction.PaymentMethodId == EnmPaymentMethod.ZaloPayGateway
            ) {
                if(rechargeWalletInput.EWallet != null)
                {
                    var walletInput = rechargeWalletInput.EWallet;
                    walletInput.TransactionId = output.TransactionId;
                    output.EWallet = await _eWalletDepositService.InsertEWalletDeposit(walletInput);
                }                
            }
            else if (rechargeWalletInput.Transaction.PaymentMethodId == EnmPaymentMethod.DebtSale)
            {
                var dbtSale = rechargeWalletInput.Debit;
                dbtSale.TransactionId = output.TransactionId;
                dbtSale.CreatedBy = output.Transaction.CreatedBy;
                output.Debit = await _debitService.InsertAsync(dbtSale);
            }
            if (UpdateAccountAmount && rechargeWalletInput.Transaction.Amount.HasValue) await _accountRepository.ChangeAmount(rechargeWalletInput.Transaction.AccountId.Value, rechargeWalletInput.Transaction.Amount.Value);
            return output;
        }

        public async Task<List<TransactionFullOutputDto>> GetByListPaymentRequestIds(List<Guid> listIds)
        {
            var list = await _transactionRepository.GetByListPaymentRequestIds(listIds);
            return ObjectMapper.Map<List<Transaction>, List<TransactionFullOutputDto>>(list);
        }

        public async Task<bool> HasTransferDepositNotIsConfirmTrans(Guid paymentRequestId, DateTime? paymentRequestDate)
        {
            return await _transactionRepository.HasTransferDepositNotIsConfirmTrans(paymentRequestId, paymentRequestDate);
        }
        public async Task<bool> HasTransferDepositNotIsConfirmTrans(Guid paymentRequestId, DateTime? paymentRequestDate, List<EnmTransactionType> transactionTypes)
        {
            return await _transactionRepository.HasTransferDepositNotIsConfirmTrans(paymentRequestId, transactionTypes, paymentRequestDate);
        }

        public async Task<decimal?> GetSumAmountOfPaymentRequest(Guid paymentRequestId, List<EnmTransactionType> transactionTypes, DateTime? paymentRequestDate)
        {
            return await _transactionRepository.GetSumAmountOfPaymentRequest(paymentRequestId, transactionTypes, paymentRequestDate);
        }
    }
}
