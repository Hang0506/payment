using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Dto.v2;
using FRTTMO.PaymentCore.Entities;
using FRTTMO.PaymentCore.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Services.v2
{
    public class TransactionServiceV2 : PaymentCoreAppService, ITransientDependency, ITransactionServiceV2
    {
        private readonly ILogger<TransactionServiceV2> _log;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IGenerateServiceV2 _generateServiceV2;
        private readonly ICardService _cardService;
        private readonly ICODService _cODService;
        private readonly ITransferService _transferService;
        private readonly IVoucherService _voucherService;
        private readonly IEWalletDepositService _eWalletDepositService;
        private readonly IAccountRepository _accountRepository;
        private IDebitService _debitService;

        public TransactionServiceV2(
            ILogger<TransactionServiceV2> log,
            ITransactionRepository transactionRepository,
            IGenerateServiceV2 generateServiceV2,
            IEWalletDepositService eWalletDepositService,
            ICardService cardService,
            ICODService cODService,
            ITransferService transferService,
            IVoucherService voucherService,
            IAccountRepository accountRepository,
            IDebitService debitService
        )
        {
            _log = log;
            _transactionRepository = transactionRepository;
            _generateServiceV2 = generateServiceV2;
            _eWalletDepositService = eWalletDepositService;
            _cardService = cardService;
            _cODService = cODService;
            _transferService = transferService;
            _voucherService = voucherService;
            _accountRepository = accountRepository;
            _debitService = debitService;
        }

        public async Task<List<TransactionFullOutputDtoV2>> GetByPaymentRequestCode(string paymentRequestCode, DateTime? paymentRequestDate)
        {
            var trans = await _transactionRepository.GetByPaymentRequestCode(paymentRequestCode, paymentRequestDate);
            var rs = trans.Select(ObjectMapper.Map<Transaction, TransactionFullOutputDtoV2>).ToList();
            return rs;
        }

        public async Task<List<TransactionFullOutputDtoV2>> GetByPaymentRequestInfo(GetByPaymentRequestInfoInputV2 infoInput)
        {
            var trans = await _transactionRepository.GetByPaymentRequestInfo(infoInput.PaymentRequestCode, infoInput.TransactionTypeIds, infoInput.PaymentRequestDate);
            var rs = trans.Select(ObjectMapper.Map<Transaction, TransactionFullOutputDtoV2>).ToList();
            return rs;
        }

        public async Task<decimal?> GetSumAmountOfPaymentRequest(GetByPaymentRequestInfoInputV2 infoInput)
        {
            return await _transactionRepository.GetSumAmountOfPaymentRequest(infoInput.PaymentRequestCode, infoInput.TransactionTypeIds, infoInput.PaymentRequestDate);
        }

        public async Task<decimal?> GetSumAmountOfPaymentRequest(string paymentRequestCode, List<EnmTransactionType> transactionTypes, DateTime? paymentRequestDate)
        {
            return await _transactionRepository.GetSumAmountOfPaymentRequest(paymentRequestCode, transactionTypes, paymentRequestDate);
        }

        public async Task<bool> HasTransferDepositNotIsConfirmTrans(string paymentRequestCode, DateTime? paymentRequestDate)
        {
            return await _transactionRepository.HasTransferDepositNotIsConfirmTrans(paymentRequestCode, paymentRequestDate);
        }

        public async Task<TransactionFullOutputDtoV2> InsertTransaction(InsertTransactionInputDtoV2 transactionInputDto)
        {
            var inputET = ObjectMapper.Map<InsertTransactionInputDtoV2, Transaction>(transactionInputDto);
            if (string.IsNullOrEmpty(inputET.ModifiedBy))
            {
                inputET.ModifiedBy = inputET.CreatedBy;
            }
            if (inputET.PaymentRequestDate == null || inputET.PaymentRequestDate.Value == default)
                inputET.PaymentRequestDate = _generateServiceV2.GetPaymentRequestDate(inputET.PaymentRequestCode);
            var outputET = await _transactionRepository.InsertTransaction(inputET);
            return ObjectMapper.Map<Transaction, TransactionFullOutputDtoV2>(outputET);
        }

        public async Task<DepositCoresOutputDtoV2> InsertTransactionWithDetail(DepositCoresInputDtoV2 rechargeWalletInput, bool UpdateAccountAmount = false)
        {
            var output = new DepositCoresOutputDtoV2();
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
            )
            {
                if (rechargeWalletInput.EWallet != null)
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
        public async Task<List<TransactionFullOutputDtoV2>> GetByPaymentRequestId(Guid paymentRequestId, DateTime? paymentRequestDate)
        {
            var trans = await _transactionRepository.GetByPaymentRequestId(paymentRequestId, paymentRequestDate);
            var rs = trans.Select(ObjectMapper.Map<Transaction, TransactionFullOutputDtoV2>).ToList();
            return rs;
        }
    }
}
