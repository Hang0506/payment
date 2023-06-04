using FRTTMO.DebitService.Services;
using FRTTMO.PaymentCore.Application.Contracts;
using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Entities;
using FRTTMO.PaymentCore.Exceptions;
using FRTTMO.PaymentCore.Kafka.Eto;
using FRTTMO.PaymentCore.Kafka.Interface;
using FRTTMO.PaymentCore.RemoteAPIs;
using FRTTMO.PaymentCore.Repositories;
using FRTTMO.PaymentIntegration.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.ExceptionHandling;
using Volo.Abp.Http.Client;
using Volo.Abp.Uow;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Services
{
    public class DepositService : PaymentCoreAppService, ITransientDependency, IDepositService
    {
        private readonly ILogger<DepositService> _log;
        private readonly IPaymentRequestRepository _paymentRequestRepository;
        private readonly ITransactionService _transactionService;
        private readonly IAccountRepository _accountRepository;
        private readonly IPublishService<BaseETO> _iPublishService;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IVendorPinService _vendorPinService;
        private readonly IUTopAppService _uTopAppService;
        private readonly ITransferService _transferService;
        private readonly ITaptapAppService _taptapAppService;
        private readonly IGotITAppService _gotITAppService;
        private readonly ILongChauService _longChauService;
        private readonly IBankTransferService _bankTransferService;
        private readonly IEWalletsAppService _eWalletsAppService;
        private readonly IInternalAppServiceCore _internalAppService;
        private readonly IVPBAppService _vpbAppService;
        private readonly IDebitAppService _debitAppService;

        public DepositService(
            ILogger<DepositService> log,
            IPaymentRequestRepository paymentRequestRepository,
            ITransactionService transactionService,
            IAccountRepository accountRepository,
            IPublishService<BaseETO> iPublishService,
            IUnitOfWorkManager unitOfWorkManager,
            IVendorPinService vendorPinService,
            IUTopAppService uTopAppService,
            ITransferService transferService,
            ITaptapAppService taptapAppService,
            IGotITAppService gotITAppService,
            IEWalletsAppService eWalletsAppService,
            IBankTransferService bankTransferService,
            ILongChauService longChauService,
            IInternalAppServiceCore internalAppServiceCore,
            IVPBAppService vPBAppService,
             IDebitAppService debitAppService
        ) : base()
        {
            _log = log;
            _paymentRequestRepository = paymentRequestRepository;
            _transactionService = transactionService;
            _accountRepository = accountRepository;
            _iPublishService = iPublishService;
            _unitOfWorkManager = unitOfWorkManager;
            _vendorPinService = vendorPinService;
            _uTopAppService = uTopAppService;
            _transferService = transferService;
            _taptapAppService = taptapAppService;
            _gotITAppService = gotITAppService;
            _eWalletsAppService = eWalletsAppService;
            _bankTransferService = bankTransferService;
            _vpbAppService = vPBAppService;
            _longChauService = longChauService;
            _internalAppService = internalAppServiceCore;
            _debitAppService = debitAppService;
        }
        private static List<EnmVoucherProvider> VoucherAvailabel = new() { EnmVoucherProvider.GotIT, EnmVoucherProvider.LC, EnmVoucherProvider.Taptap, EnmVoucherProvider.UTop };
        private static Dictionary<EnmWalletProvider, PaymentIntegration.Common.EnumType.EnmWalletProvider> EwalletAvailabel = new()
        {
            { EnmWalletProvider.VNPAY, PaymentIntegration.Common.EnumType.EnmWalletProvider.VNPAY },
            { EnmWalletProvider.Smartpay, PaymentIntegration.Common.EnumType.EnmWalletProvider.Smartpay },
            { EnmWalletProvider.ShopeePay, PaymentIntegration.Common.EnumType.EnmWalletProvider.ShopeePay },
            { EnmWalletProvider.Zalopay, PaymentIntegration.Common.EnumType.EnmWalletProvider.Zalopay },
            { EnmWalletProvider.Foxpay, PaymentIntegration.Common.EnumType.EnmWalletProvider.Foxpay },
        };
        private async Task<OMS_OrderResult> InitBeforDeposit(EnmPaymentMethod paymentMethod, DepositCoresInputDto inItem)
        {
            AddLogElapsed("StartInitBeforDeposit");
            if (string.IsNullOrEmpty(inItem.OrderCode)) throw new BusinessException(PaymentCoreErrorCodes.ERROR_ORDER_CODE_NOT_EMPTY);
            if (inItem.Transaction == null) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION)
                                                        .WithData(PaymentCoreErrorMessageKey.OrderCode, ParamTypes.JustData, inItem.OrderCode)
                                                        .WithData("Detail", ParamTypes.JustData, "Transactions bị null")
                                                        .WithData("Data", "Transactions");
            if (inItem.Transaction.PaymentMethodId != paymentMethod) throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_METHOD_INVALID);
            if ((inItem.Transaction.Amount ?? 0m) < 0m) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_ENTITY_INVALID)
                                                             .WithData(PaymentCoreErrorMessageKey.OrderCode, ParamTypes.JustData, inItem.OrderCode)
                                                             .WithData("TransactionAmount", ParamTypes.JustData, inItem.Transaction.Amount)
                                                             .WithData("Entity", "Transaction Amount");
            if (inItem.Transaction.PaymentMethodId == EnmPaymentMethod.Wallet)
            {
                if (inItem.EWallet == null) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION)
                                                        .WithData(PaymentCoreErrorMessageKey.OrderCode, ParamTypes.JustData, inItem.OrderCode)
                                                        .WithData("Detail", ParamTypes.JustData, "Không thấy thông tin eWallet!")
                                                        .WithData("Data", "Thông tin eWallet");
                if (inItem.EWallet.Amount != inItem.Transaction.Amount) throw new BusinessException(PaymentCoreErrorCodes.ERROR_MONNEY_PAYMENT_NOTEQUAL).WithData("Wallet", inItem.EWallet);
                if ((inItem.EWallet.Amount ?? 0m) < 0m) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_ENTITY_INVALID)
                                                                 .WithData(PaymentCoreErrorMessageKey.OrderCode, ParamTypes.JustData, inItem.OrderCode)
                                                                 .WithData("EWalletAmount", ParamTypes.JustData, inItem.EWallet.Amount)
                                                                 .WithData("Entity", "eWallet Amount");
            }
            else if (inItem.Transaction.PaymentMethodId == EnmPaymentMethod.Voucher)
            {
                if (inItem.Vouchers == null || !inItem.Vouchers.Any()) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION)
                                                        .WithData(PaymentCoreErrorMessageKey.OrderCode, ParamTypes.JustData, inItem.OrderCode)
                                                        .WithData("Detail", ParamTypes.JustData, "Không thấy thông tin Voucher!")
                                                        .WithData("Data", "Không thấy thông tin Voucher!");
                if (inItem.Vouchers.Sum(c => c.Amount ?? 0m) != inItem.Transaction.Amount) throw new BusinessException(PaymentCoreErrorCodes.ERROR_MONNEY_PAYMENT_NOTEQUAL).WithData("Voucher", inItem.Vouchers);
                if (inItem.Vouchers.Any(c => c.Amount.HasValue && c.Amount < 0m)) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_ENTITY_INVALID)
                                                                .WithData(PaymentCoreErrorMessageKey.OrderCode, ParamTypes.JustData, inItem.OrderCode)
                                                                .WithData("Vouchers", ParamTypes.JustData, inItem.Vouchers)
                                                                .WithData("Entity", "Vouchers Amount");
                var sh = inItem.Vouchers.GroupBy(c => new { c.VoucherType, c.Code }).Select(c => new { c.Key.VoucherType, c.Key.Code, CountVC = c.Count() });
                if (sh.Any(c => c.CountVC > 1)) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_EXCEPTION)
                                                         .WithData(PaymentCoreErrorMessageKey.OrderCode, ParamTypes.JustData, inItem.OrderCode)
                                                         .WithData("Detail", ParamTypes.JustData, "Không được nhập trùng VoucherCode")
                                                         .WithData("Voucher", inItem.Vouchers);
            }
            else if (inItem.Transaction.PaymentMethodId == EnmPaymentMethod.Card)
            {
                if (inItem.Cards == null || !inItem.Cards.Any()) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION)
                                                        .WithData(PaymentCoreErrorMessageKey.OrderCode, ParamTypes.JustData, inItem.OrderCode)
                                                        .WithData("Detail", ParamTypes.JustData, "Không thấy thông tin Card!")
                                                        .WithData("Data", "Thông tin Card");
                if (inItem.Cards.Sum(c => c.Amount ?? 0m) != inItem.Transaction.Amount) throw new BusinessException(PaymentCoreErrorCodes.ERROR_MONNEY_PAYMENT_NOTEQUAL).WithData("Card", inItem.Cards);
                if (inItem.Cards.Any(c => c.Amount.HasValue && c.Amount < 0m)) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_ENTITY_INVALID)
                                                                .WithData(PaymentCoreErrorMessageKey.OrderCode, ParamTypes.JustData, inItem.OrderCode)
                                                                .WithData("Cards", ParamTypes.JustData, inItem.Cards)
                                                                .WithData("Entity", "Cards Amount");
            }
            else if (inItem.Transaction.PaymentMethodId == EnmPaymentMethod.COD)
            {
                if (inItem.COD == null) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION)
                                                        .WithData(PaymentCoreErrorMessageKey.OrderCode, ParamTypes.JustData, inItem.OrderCode)
                                                        .WithData("Detail", ParamTypes.JustData, "Không thấy thông tin COD!")
                                                        .WithData("Data", "Thông tin COD");
                if (inItem.COD.Amount != inItem.Transaction.Amount) throw new BusinessException(PaymentCoreErrorCodes.ERROR_MONNEY_PAYMENT_NOTEQUAL).WithData("COD", inItem.COD);
                if ((inItem.COD.Amount ?? 0m) < 0m) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_ENTITY_INVALID)
                                                                .WithData(PaymentCoreErrorMessageKey.OrderCode, ParamTypes.JustData, inItem.OrderCode)
                                                                .WithData("COD", ParamTypes.JustData, inItem.COD)
                                                                 .WithData("Entity", "COD Amount");
            }
            else if (inItem.Transaction.PaymentMethodId == EnmPaymentMethod.Transfer)
            {
                if (inItem.Transfers == null || !inItem.Transfers.Any()) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION)
                                                        .WithData(PaymentCoreErrorMessageKey.OrderCode, ParamTypes.JustData, inItem.OrderCode)
                                                        .WithData("Detail", ParamTypes.JustData, "Không thấy thông tin Transfer!")
                                                        .WithData("Data", "Thông tin Transfer");
                if (inItem.Transfers.Sum(c => c.Amount ?? 0m) != inItem.Transaction.Amount) throw new BusinessException(PaymentCoreErrorCodes.ERROR_MONNEY_PAYMENT_NOTEQUAL).WithData("Transfer", inItem.Transfers);
                if (inItem.Transfers.Any(c => c.Amount.HasValue && c.Amount < 0m)) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_ENTITY_INVALID)
                                                                       .WithData(PaymentCoreErrorMessageKey.OrderCode, ParamTypes.JustData, inItem.OrderCode)
                                                                       .WithData("Transfers", ParamTypes.JustData, inItem.Transfers)
                                                                       .WithData("Entity", "Transfers Amount");
                var dup = inItem.Transfers.Where(c => !string.IsNullOrWhiteSpace(c.TransferNum)).GroupBy(c => c.TransferNum).Where(c => c.Count() > 1).Select(c => c.Key).ToList();
                if (dup.Any())
                {
                    throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_DUPLICATE_EXCEPTION).WithData("Transfernums", dup);
                }
                AddLogElapsed("CheckTransfernum");
                foreach (var chil in inItem.Transfers.Where(c => !string.IsNullOrWhiteSpace(c.TransferNum)))
                {
                    var chkNum = await _transferService.CheckTransferNum(chil.TransferNum);
                    if (chkNum) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_TRANSFERNUM_EXISTED).WithData("transfernum", chil.TransferNum);
                }
            }
            else if (
                inItem.Transaction.PaymentMethodId == EnmPaymentMethod.VNPayGateway
                || inItem.Transaction.PaymentMethodId == EnmPaymentMethod.AlepayGateway
                || inItem.Transaction.PaymentMethodId == EnmPaymentMethod.MocaEWallet
            )
            {
                if (inItem.EWallet == null) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION)
                                                        .WithData(PaymentCoreErrorMessageKey.OrderCode, ParamTypes.JustData, inItem.OrderCode)
                                                        .WithData("Detail", ParamTypes.JustData, "Không thấy thông tin eWallet!")
                                                        .WithData("Data", "Thông tin eWallet");
                if (inItem.EWallet.Amount != inItem.Transaction.Amount) throw new BusinessException(PaymentCoreErrorCodes.ERROR_MONNEY_PAYMENT_NOTEQUAL).WithData("Wallet", inItem.EWallet);
                if ((inItem.EWallet.Amount ?? 0m) < 0m) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_ENTITY_INVALID)
                                                                 .WithData(PaymentCoreErrorMessageKey.OrderCode, ParamTypes.JustData, inItem.OrderCode)
                                                                 .WithData("EWalletAmount", ParamTypes.JustData, inItem.EWallet.Amount)
                                                                 .WithData("Entity", "eWallet Amount");
            }
            //Step1: Get OMS.OrderInfo by OrderID
            if (!inItem.Transaction.PaymentRequestId.HasValue) throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_REQUEST_NOTFOUND);

            AddLogElapsed("GetPaymentRQById");
            var chkPayRQ = await _paymentRequestRepository.GetById(inItem.Transaction.PaymentRequestId.Value);
            if (chkPayRQ == null) throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_REQUEST_NOTFOUND)
                                                          .WithData(PaymentCoreErrorMessageKey.OrderCode, inItem.OrderCode)
                                                          .WithData(PaymentCoreErrorMessageKey.PaymentRequestId, inItem.Transaction.PaymentRequestId);
            if ((chkPayRQ.Status == null) || (chkPayRQ.Status != EnmPaymentRequestStatus.Confirm)) throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_PAYMENTREQUEST_STATUS)
                                                            .WithData(PaymentCoreErrorMessageKey.OrderCode, inItem.OrderCode)
                                                            .WithData("Status", chkPayRQ?.Status)
                                                            .WithData(PaymentCoreErrorMessageKey.PaymentRequestId, inItem.Transaction.PaymentRequestId);
            inItem.Transaction.PaymentRequestDate = chkPayRQ.PaymentRequestDate;
            inItem.Transaction.PaymentRequestCode = chkPayRQ.PaymentRequestCode;
            AddLogElapsed("GetOrderOMS");
            var omsOrder = new OMS_OrderResult
            {
                OrderCode = inItem.OrderCode,
                TotalPayment = chkPayRQ.TotalPayment
            };

            AddLogElapsed("GetTotalDeposited");
            var totalTransed = await _transactionService.GetTotalDeposited(inItem.Transaction.PaymentRequestId.Value, inItem.Transaction.PaymentRequestDate);
            var totalNeedDeposit = chkPayRQ.TotalPayment - (totalTransed ?? 0m);
            if (totalNeedDeposit < 0) throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_DEPOSIT_REMAINMONNEY_INVALID)
                                                             .WithData(PaymentCoreErrorMessageKey.OrderCode, inItem.OrderCode)
                                                             .WithData(PaymentCoreErrorMessageKey.PaymentRequestId, inItem.Transaction.PaymentRequestId)
                                                             .WithData("TotalPayment", chkPayRQ.TotalPayment)
                                                             .WithData("Deposited", totalTransed);

            if (inItem.Transaction.PaymentMethodId == EnmPaymentMethod.Voucher)
            {
                AddLogElapsed("ValidationVoucher");
                var vchers = new List<VoucherInputDto>();
                //Validate Voucher
                if (inItem.Vouchers.Any(c => !c.VoucherType.HasValue || !VoucherAvailabel.Contains(c.VoucherType.Value))) throw new BusinessException(PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", "Chỉ chấp nhận Voucher của Taptap, GotIT, LC.");
                if (inItem.Vouchers.Any(c => c.VoucherType == EnmVoucherProvider.GotIT))
                {
                    foreach (var chil in inItem.Vouchers.Where(c => c.VoucherType == EnmVoucherProvider.GotIT))
                    {
                        var chkGotIT = await _gotITAppService.Validation(new PaymentIntegration.Dto.GotITValidationVoucherInputDto
                        {
                            Code = chil.Code,
                            Storecode = inItem.Transaction.ShopCode
                        });
                        if (!chkGotIT.IsValidated)
                            throw new BusinessException(PaymentIntegration.PaymentIntegrationErrorCodes.ERROR_PAYMENT_VOUCHER_CODE_CANNOTBEUSED)
                                  .WithData("voucherCode", chil.Code)
                                  .WithData("Voucher", chkGotIT);
                        if (!string.IsNullOrEmpty(chkGotIT.ExpiryDate))
                        {
                            var expD = DateTime.ParseExact(chkGotIT.ExpiryDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            if (DateTime.Now.Date > expD) throw new BusinessException(PaymentIntegration.PaymentIntegrationErrorCodes.ERROR_PAYMENT_VOUCHER_CODE_CANNOTBEUSED)
                                    .WithData("voucherCode", chil.Code)
                                    .WithData("Voucher", chkGotIT)
                                    .WithData("Detail", $"Voucher {chil.Code} hết hạn sử dụng ({expD:dd/MM/yyyy})");
                        }
                        vchers.Add(new VoucherInputDto
                        {
                            VoucherType = chil.VoucherType,
                            Code = chil.Code,
                            Amount = Convert.ToDecimal(chkGotIT.Product.Value)
                        });
                    }
                }
                else if (inItem.Vouchers.Any(c => c.VoucherType == EnmVoucherProvider.Taptap))
                {
                    foreach (var chil in inItem.Vouchers.Where(c => c.VoucherType == EnmVoucherProvider.Taptap))
                    {
                        var chkTaptap = await _taptapAppService.Validation(new PaymentIntegration.Dto.TapTapValidationVoucherInputDto
                        {
                            Code = chil.Code,
                            Storecode = inItem.Transaction.ShopCode,
                        });
                        if (!chkTaptap.IsValidated) throw new BusinessException(PaymentIntegration.PaymentIntegrationErrorCodes.ERROR_PAYMENT_VOUCHER_CODE_CANNOTBEUSED)
                               .WithData("voucherCode", chil.Code)
                               .WithData("Voucher", chkTaptap);
                        DateTime? startdate = string.IsNullOrEmpty(chkTaptap.Data.startDate) ? null : DateTime.Parse(chkTaptap.Data.startDate, CultureInfo.InvariantCulture).ToUniversalTime();//Taptap return DateTime+7 nhưng format theo kiểu UTC :(((
                        DateTime? enddate = string.IsNullOrEmpty(chkTaptap.Data.endDate) ? null : DateTime.Parse(chkTaptap.Data.endDate, CultureInfo.InvariantCulture).ToUniversalTime();//Taptap return DateTime+7 nhưng format theo kiểu UTC :(((
                        if ((startdate.HasValue && DateTime.Now < startdate) || (enddate.HasValue && DateTime.Now > enddate)) throw new BusinessException(PaymentIntegration.PaymentIntegrationErrorCodes.ERROR_PAYMENT_VOUCHER_CODE_CANNOTBEUSED)
                                  .WithData("voucherCode", chil.Code)
                                  .WithData("Voucher", chkTaptap)
                                  .WithData("Detail", $"Voucher {chil.Code} chỉ sử dụng được trong thời gian {startdate:dd/MM/yyyy HH:mm:ss} đến {enddate:dd/MM/yyyy HH:mm:ss}.");
                        if ((chkTaptap.Data.requireMinAmount > 0 && chkPayRQ.TotalPayment < chkTaptap.Data.requireMinAmount) || (chkTaptap.Data.requireMaxAmount > 0 && chkPayRQ.TotalPayment > chkTaptap.Data.requireMaxAmount))
                            throw new BusinessException(PaymentIntegration.PaymentIntegrationErrorCodes.ERROR_PAYMENT_VOUCHER_CODE_CANNOTBEUSED)
                                   .WithData("voucherCode", chil.Code)
                                   .WithData("Voucher", chkTaptap)
                                   .WithData("Detail", $"Giá trị đơn hàng không nằm trong requireMinAmount và requireMinAmount");
                        var nit = new VoucherInputDto
                        {
                            VoucherType = chil.VoucherType,
                            Code = chil.Code,
                            Amount = chkTaptap.Data.valueType == 2 ? Math.Round((decimal)chkTaptap.Data.percent * chkPayRQ.TotalPayment * 0.01m, 6) : chkTaptap.Data.value
                        };
                        if (chkTaptap.Data.valueMax > 0 && nit.Amount > chkTaptap.Data.valueMax) nit.Amount = chkTaptap.Data.valueMax;
                        vchers.Add(nit);
                    }
                }
                else if (inItem.Vouchers.Any(c => c.VoucherType == EnmVoucherProvider.LC))
                {
                    AddLogElapsed("VerifyVCLC");
                    var chkLC = await _longChauService.VoucherVerify(inItem.Vouchers.Where(c => c.VoucherType == EnmVoucherProvider.LC).Select(c => new PaymentIntegration.Dto.LongChauVoucherVerifyInputDto
                    {
                        voucherCode = c.Code
                    }).ToList());
                    AddLogElapsed("EndVerifyVCLC");
                    if (chkLC == null || chkLC.Any(c => !c.IsValidated))
                        throw new BusinessException(PaymentIntegration.PaymentIntegrationErrorCodes.ERROR_PAYMENT_VOUCHER_CODE_CANNOTBEUSED)
                            .WithData("voucherCode", string.Join(",", chkLC.Where(c => !c.IsValidated).Select(c => c.VoucherCode)))
                            .WithData("Vouchers", chkLC);
                    if (chkLC.Any(c => c.TypeCode != "2"))
                        throw new BusinessException(PaymentIntegration.PaymentIntegrationErrorCodes.ERROR_PAYMENT_VOUCHER_CODE_CANNOTBEUSED)
                                  .WithData("voucherCode", string.Join(",", chkLC.Where(c => c.TypeCode != "2").Select(c => c.VoucherCode)))
                                  .WithData("Vouchers", chkLC)
                                  .WithData("Detail", $"Vouchers có Type khác 2.");
                    vchers.AddRange(chkLC.Select(c => new VoucherInputDto
                    {
                        VoucherType = EnmVoucherProvider.LC,
                        Code = c.VoucherCode,
                        Amount = c.AmountMoney
                    }));
                }
                else if (inItem.Vouchers.Any(c => c.VoucherType == EnmVoucherProvider.UTop))
                {
                    var chkUtop = await _uTopAppService.Validation(new PaymentIntegration.Dto.UTopValidationVoucherInputDto
                    {
                        VoucherCodes = inItem.Vouchers.Where(c => c.VoucherType == EnmVoucherProvider.UTop).Select(c => c.Code).ToList()
                    });
                    if (!chkUtop.IsValidate)
                        throw new BusinessException(PaymentIntegration.PaymentIntegrationErrorCodes.ERROR_PAYMENT_VOUCHER_CODE_CANNOTBEUSED)
                                  .WithData("voucherCode", string.Join(",", inItem.Vouchers.Where(c => c.VoucherType == EnmVoucherProvider.UTop).Select(c => c.Code)))
                                  .WithData("Vouchers", chkUtop);
                    var lstInvaild = chkUtop.Results.Where(vc => DateTime.UtcNow > vc.ExpiredDay).Select(c => c.VoucherCode).ToList();
                    if (lstInvaild.Any()) throw new BusinessException(PaymentIntegration.PaymentIntegrationErrorCodes.ERROR_PAYMENT_VOUCHER_CODE_CANNOTBEUSED)
                                  .WithData("voucherCode", string.Join(",", lstInvaild))
                                  .WithData("Detail", $"Voucher quá hạn sử dụng.");
                    vchers.AddRange(chkUtop.Results.Select(c => new VoucherInputDto
                    {
                        VoucherType = EnmVoucherProvider.UTop,
                        Code = c.VoucherCode,
                        Amount = c.UtopAmount
                    }));
                }
                var VcAmountInvail = inItem.Vouchers.Where(c => !vchers.Any(v => v.VoucherType == c.VoucherType && v.Code == c.Code && c.Amount <= v.Amount)).Select(c => c.Code).ToList();
                if (VcAmountInvail.Any()) throw new BusinessException(PaymentIntegration.PaymentIntegrationErrorCodes.ERROR_PAYMENT_VOUCHER_CODE_CANNOTBEUSED)
                                 .WithData("voucherCode", string.Join(",", VcAmountInvail))
                                 .WithData("Detail", $"Giá trị Input vượt quá Giá trị cho phép của Voucher.");

                //Check Data
                if (inItem.Transaction.Amount < totalNeedDeposit)
                {
                    inItem.Vouchers.ForEach(c =>
                    {
                        if (vchers.Any(vc => c.VoucherType == vc.VoucherType && c.Code == vc.Code && c.Amount != vc.Amount))
                            throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_DEPOSIT_VOUCHER_AMOUNT_NOTMATCH_AMOUNT_VERIFY)
                                                           .WithData(PaymentCoreErrorMessageKey.OrderCode, ParamTypes.JustData, inItem.OrderCode)
                                                           .WithData("Data", c.Code);
                    });
                }
                else if (inItem.Transaction.Amount > totalNeedDeposit)
                {
                    inItem.Transaction.Amount = totalNeedDeposit;
                    var AmountRem = inItem.Transaction.Amount;
                    inItem.Vouchers.ForEach(c =>
                    {
                        if (AmountRem > 0m && AmountRem >= c.Amount)
                        {
                            AmountRem -= c.Amount;
                        }
                        else if (AmountRem > 0m && AmountRem < c.Amount)
                        {
                            c.Amount = AmountRem;
                            AmountRem = 0m;
                        }
                        else
                        {
                            c.Amount = 0m;
                        }
                    });
                }
            }
            else if (inItem.Transaction.PaymentMethodId == EnmPaymentMethod.Transfer)
            {
                AddLogElapsed("StartValidationTransfer");
                if (inItem.Transfers.Any(c => c.IsConfirm == EnmTransferIsConfirm.AdvanceTransfer))
                {
                    if (inItem.Transfers.Where(c => c.PartnerId.HasValue && c.PartnerId == EnmPartnerId.VPB).Count() == inItem.Transfers.Count)
                    {
                        foreach (var chil in inItem.Transfers.Where(c => c.PartnerId.HasValue && c.PartnerId == EnmPartnerId.VPB))
                        {
                            var srhVPB = await _vpbAppService.CheckTransaction(new PaymentIntegration.Dto.VPB.VPBCheckTransactionInputDto
                            {
                                Amount = Convert.ToInt64(chil.Amount),
                                FromDateTime = chil.DateTranfer.Value.Date,
                                ToDateTime = chil.DateTranfer.Value.Date.AddDays(1).AddSeconds(-1),
                                VirtualAccNo = chil.TransferNum
                            });
                            if (srhVPB == null || !srhVPB.Any() || !srhVPB[0].IsPayed || srhVPB[0].Amount != chil.Amount)
                            {
                                throw new BusinessException(PaymentCoreErrorCodes.ERROR_MONNEY_PAYMENT_NOTEQUAL)
                                             .WithData(PaymentCoreErrorMessageKey.MessageDetail, "Số tiền Input khác với số tiền Bank trả về.")
                                             .WithData("Transfers", chil)
                                             .WithData("SMS", srhVPB)
                                             ;
                            }
                        }
                    }
                    else
                    {
                        var sms = await _bankTransferService.GetSmsBanking(new PaymentIntegration.Dto.BankTransferGetSmsBankingInputDto
                        {
                            OrderCode = omsOrder.OrderCode,
                            PhoneNumber = inItem.Phone,
                            Shopcode = inItem.Transaction.ShopCode,
                        });
                        var srh = inItem.Transfers.Where(c => c.IsConfirm == EnmTransferIsConfirm.AdvanceTransfer && !sms.Any(s => s.TransactionId == c.TransferNum && s.Amount == c.Amount)).ToList();
                        if (srh.Any()) throw new BusinessException(PaymentCoreErrorCodes.ERROR_MONNEY_PAYMENT_NOTEQUAL)
                                 .WithData(PaymentCoreErrorMessageKey.MessageDetail, "Số tiền Input khác với số tiền Bank trả về.")
                                  .WithData("Transfers", srh)
                                  .WithData("SMS", sms)
                                  ;
                    }
                }
                if (inItem.Transfers.Any(c => c.IsConfirm.HasValue && c.IsConfirm == EnmTransferIsConfirm.AdvanceTransfer))
                {
                    if ((inItem.Transaction.Amount - totalNeedDeposit) > 5000m) throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_DEPOSITMONEY_MORETHAN_REMAINMONEY)
                                                                           .WithData(PaymentCoreErrorMessageKey.OrderCode, inItem.OrderCode)
                                                                           .WithData(PaymentCoreErrorMessageKey.PaymentRequestId, inItem.Transaction.PaymentRequestId)
                                                                           .WithData("TransactionAmount", inItem.Transaction.Amount)
                                                                           .WithData("PaymentRequest-TotalPayment", chkPayRQ.TotalPayment)
                                                                           .WithData("Deposited", totalTransed)
                                                                           .WithData("IsConfirm", 0);
                }
                else
                {
                    if (inItem.Transaction.Amount > totalNeedDeposit) throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_DEPOSITMONEY_MORETHAN_REMAINMONEY)
                                                                           .WithData(PaymentCoreErrorMessageKey.OrderCode, inItem.OrderCode)
                                                                           .WithData(PaymentCoreErrorMessageKey.PaymentRequestId, inItem.Transaction.PaymentRequestId)
                                                                           .WithData("TransactionAmount", inItem.Transaction.Amount)
                                                                           .WithData("PaymentRequest-TotalPayment", chkPayRQ.TotalPayment)
                                                                           .WithData("Deposited", totalTransed);
                }
            }
            else if (inItem.Transaction.PaymentMethodId == EnmPaymentMethod.DebtSale)
            {
                if (inItem.Debit == null) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION)
                                                        .WithData(PaymentCoreErrorMessageKey.OrderCode, ParamTypes.JustData, inItem.OrderCode)
                                                        .WithData("Detail", ParamTypes.JustData, "Không thấy thông tin Debit!")
                                                        .WithData("Data", "Thông tin Debit");
                if (inItem.Debit.Amount != inItem.Transaction.Amount) throw new BusinessException(PaymentCoreErrorCodes.ERROR_MONNEY_PAYMENT_NOTEQUAL).WithData("Debit", inItem.Debit);
                if ((inItem.Debit.Amount ?? 0m) < 0m) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_ENTITY_INVALID)
                                                                .WithData(PaymentCoreErrorMessageKey.OrderCode, ParamTypes.JustData, inItem.OrderCode)
                                                                .WithData("Debit", ParamTypes.JustData, inItem.COD)
                                                                 .WithData("Entity", "Debit Amount");
                if (inItem.Debit.TaxCode == null && inItem.Phone == null) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_ENTITY_INVALID)
                                                                .WithData(PaymentCoreErrorMessageKey.OrderCode, ParamTypes.JustData, inItem.OrderCode)
                                                                .WithData("TaxCode and Phone", ParamTypes.JustData, "không hợp lệ")
                                                                 .WithData("Entity", "Debit TaxCode,Phone");

            }
            else
            {
                if (inItem.Transaction.Amount > totalNeedDeposit) throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_DEPOSITMONEY_MORETHAN_REMAINMONEY)
                                                                       .WithData(PaymentCoreErrorMessageKey.OrderCode, inItem.OrderCode)
                                                                       .WithData(PaymentCoreErrorMessageKey.PaymentRequestId, inItem.Transaction.PaymentRequestId)
                                                                       .WithData("TransactionAmount", inItem.Transaction.Amount)
                                                                       .WithData("PaymentRequest-TotalPayment", chkPayRQ.TotalPayment)
                                                                       .WithData("Deposited", totalTransed);
            }

            AddLogElapsed("CheckAccount");
            //Step2:  Check account ID in table account
            if (!inItem.Transaction.AccountId.HasValue) throw new BusinessException(PaymentCoreErrorCodes.ERROR_ACCOUNT_NOTFOUND);
            var extAc = await _accountRepository.GetById(inItem.Transaction.AccountId.Value);
            if (extAc == null) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_ACCOUNT_NOTFOUND).WithData(PaymentCoreErrorMessageKey.AccountId, inItem.Transaction.AccountId);
            if (inItem.Transaction.TransactionTypeId != EnmTransactionType.FirstDeposit && inItem.Transaction.TransactionTypeId != EnmTransactionType.WithdrawDeposit)
            {
                inItem.Transaction.TransactionTypeId = EnmTransactionType.Recharge;
            }
            omsOrder.PaymentRequestCode = chkPayRQ.PaymentRequestCode;

            return omsOrder;
        }
        /// <summary>
        /// Nạp tiền vào tài khoản ví khách hàng bằng tiền mặt
        /// </summary>
        public async Task<DepositByCashOutputDto> DepositByCash(DepositByCashInputDto inItem)
        {
            var obj = ObjectMapper.Map<DepositByCashInputDto, DepositCoresInputDto>(inItem);
            var ord = await InitBeforDeposit(EnmPaymentMethod.Cash, obj);
            //Insert  transaction
            DepositByCashOutputDto rt;
            using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true))
            {
                try
                {
                    AddLogElapsed("InsertTransDetail");
                    var trans = await _transactionService.InsertTransactionWithDetail(obj, true);
                    AddLogElapsed("MapReturn");
                    rt = ObjectMapper.Map<DepositWalletOutputDto, DepositByCashOutputDto>(trans);
                    await unitOfWork.SaveChangesAsync();

                    //var inputOrdercode = new SearchESByOrderCodeRequestDto();//
                    //var requestOms = new Dictionary<string, object>();
                    //requestOms.Add("PaymentCode", inItem.PaymentCode);
                    //var orderInfo = await _internalAppService.InvokeApi<List<SearchESOrder_OutPutDto>>(
                    //                              EnvironmentSetting.RemoteOMSService,
                    //                              InternalOMSApiUrl.orderInfo,
                    //                              RestSharp.Method.GET,
                    //                              null, requestOms);
                    //if (orderInfo.Count != 0 && orderInfo.FirstOrDefault().ShipmentPlannings.Count > 0)
                    //{
                    //    if (orderInfo.FirstOrDefault().ShipmentPlannings.LastOrDefault().CarrierName == "Long Châu Delivery")
                    //    {
                    //        // call Api AR update-status (HasBeenDeducted)
                    //        var accountingOutput = await _internalAppService.InvokeInternalAPI_GetData<List<AccountingDetailOutputDto>>
                    //            (EnvironmentSetting.RemoteDebitService, $"api/DebitService/accounting/by-order-code/{inItem.PaymentCode}");
                    //        // call Api AR update-status (HasBeenDeducted)
                    //        if (accountingOutput.Count != 0)
                    //        {
                    //            var inputRequest = new AccountingByCashInputDto
                    //            {
                    //                AccountingCode = accountingOutput.FirstOrDefault().AccountingCode,
                    //                PaymentCode = inItem.PaymentCode,
                    //                Type = 1,
                    //                CashAmount = accountingOutput.FirstOrDefault().TotalAmount,
                    //                Description = inItem.Transaction != null ? inItem.Transaction.Note : "",
                    //                EmployeeId = inItem.EmployeeId ?? "",
                    //                EmployeeName = inItem.EmployeeName ?? "",
                    //                PaymentMethod = "COD",
                    //                AccountingBy = "00000",
                    //                AccountingName = "Hệ thống tự hạch toán"
                    //            };
                    //            await _internalAppService.InvokeApi<object>(
                    //                              EnvironmentSetting.RemoteDebitService,
                    //                              InternalApiUrl.hasBeenDeducted,
                    //                              RestSharp.Method.POST,
                    //                              JsonConvert.SerializeObject(inputRequest)
                    //                          );
                    //        }
                    //    }
                    //}
                }
                catch (Exception)
                {
                    await unitOfWork.RollbackAsync();
                    throw;
                }
            }
            await _iPublishService.ProduceAsync(new TransactionCreatedETO
            {
                OrderCode = ord.OrderCode,
                Data = rt
            });
            if (inItem.Transaction.TransactionTypeId == EnmTransactionType.FirstDeposit)
            {
                await _iPublishService.ProduceAsync(new CollectDepositTransactionCreatedETO
                {
                    OrderCode = ord.OrderCode,
                    Data = rt
                });
            }
            return rt;
        }
        public async Task<DepositByEWalletOutputDto> DepositByEWallet(DepositByEWalletInputDto inItem)
        {
            var obj = ObjectMapper.Map<DepositByEWalletInputDto, DepositCoresInputDto>(inItem);
            var ord = await InitBeforDeposit(EnmPaymentMethod.Wallet, obj);
            //Check VNPay, SmartPay
            if (!EwalletAvailabel.ContainsKey(obj.EWallet.TypeWalletId.Value)) throw new BusinessException(PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", $"Không hỗ trợ ví {Enum.GetName(obj.EWallet.TypeWalletId.Value)}.");

            AddLogElapsed("CheckPaiedEWallet");
            var ChkPayed = await _eWalletsAppService.CheckTransactionPayed(new PaymentIntegration.Dto.CheckTransactionEwalletInputDto
            {
                PayDate = inItem.EWallet.CreatedDate.Value,
                PaymentRequestCode = ord.PaymentRequestCode,
                ShopCode = inItem.Transaction.ShopCode,
                ProviderId = EwalletAvailabel[obj.EWallet.TypeWalletId.Value],
                Amount = Convert.ToInt64(obj.EWallet.Amount.Value),
                TransactionType = obj.EWallet.TypeWalletId == EnmWalletProvider.ShopeePay ? (uint)PaymentIntegration.Dto.ShopeePay.ShopeePayTransactionType.Payment : default
            });
            AddLogElapsed("EndCheckPaiedEWallet");
            if (ChkPayed == null) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION)
                    .WithData("ProviderResult", ParamTypes.JustData, ChkPayed)
                    .WithData("ProviderResult", "Provider Result");
            if (!ChkPayed.IsPayed) throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_EXCEPTION)
                    .WithData("ProviderResult", ParamTypes.JustData, ChkPayed)
                    .WithData("Message", "Order chưa được thanh toán!");
            var amtPayed = ChkPayed.DebitAmount ?? ChkPayed.RealAmount;
            if (obj.EWallet.Amount != amtPayed) throw new BusinessException(PaymentCoreErrorCodes.ERROR_MONNEY_PAYMENT_NOTEQUAL)
                      .WithData("DebitAmount", ChkPayed.DebitAmount)
                      .WithData("RealAmount", ChkPayed.RealAmount)
                      .WithData("ProviderResult", ChkPayed)
                      ;
            obj.EWallet.RealAmount = ChkPayed.RealAmount ?? ChkPayed.DebitAmount;
            //Insert  transaction
            DepositByEWalletOutputDto rt;
            using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true))
            {
                try
                {
                    AddLogElapsed("InsertTransDetail");
                    var trans = await _transactionService.InsertTransactionWithDetail(obj, true);
                    AddLogElapsed("MapReturn");
                    rt = ObjectMapper.Map<DepositWalletOutputDto, DepositByEWalletOutputDto>(trans);
                    await unitOfWork.SaveChangesAsync();
                }
                catch (Exception)
                {
                    await unitOfWork.RollbackAsync();
                    throw;
                }
            }
            await _iPublishService.ProduceAsync(new TransactionCreatedETO
            {
                OrderCode = ord.OrderCode,
                Data = rt
            });
            if (inItem.Transaction.TransactionTypeId == EnmTransactionType.FirstDeposit)
            {
                await _iPublishService.ProduceAsync(new CollectDepositTransactionCreatedETO
                {
                    OrderCode = ord.OrderCode,
                    Data = rt
                });
            }
            return rt;
        }
        public async Task<DepositByCardOutputDto> DepositByCard(DepositByCardInputDto inItem)
        {
            var obj = ObjectMapper.Map<DepositByCardInputDto, DepositCoresInputDto>(inItem);
            var ord = await InitBeforDeposit(EnmPaymentMethod.Card, obj);
            //Insert  transaction
            DepositByCardOutputDto rt;
            using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true))
            {
                try
                {
                    AddLogElapsed("InsertTransDetail");
                    var trans = await _transactionService.InsertTransactionWithDetail(obj, true);
                    AddLogElapsed("MapReturn");
                    rt = ObjectMapper.Map<DepositWalletOutputDto, DepositByCardOutputDto>(trans);
                    await unitOfWork.SaveChangesAsync();
                }
                catch (Exception)
                {
                    await unitOfWork.RollbackAsync();
                    throw;
                }
            }
            await _iPublishService.ProduceAsync(new TransactionCreatedETO
            {
                OrderCode = ord.OrderCode,
                Data = rt
            });
            if (inItem.Transaction.TransactionTypeId == EnmTransactionType.FirstDeposit)
            {
                await _iPublishService.ProduceAsync(new CollectDepositTransactionCreatedETO
                {
                    OrderCode = ord.OrderCode,
                    Data = rt
                });
            }
            return rt;
        }
        public async Task<DepositByCODOutputDto> DepositByCOD(DepositByCODInputDto inItem)
        {
            var obj = ObjectMapper.Map<DepositByCODInputDto, DepositCoresInputDto>(inItem);
            var ord = await InitBeforDeposit(EnmPaymentMethod.COD, obj);
            //Insert  transaction
            DepositByCODOutputDto rt;
            using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true))
            {
                try
                {
                    AddLogElapsed("InsertTransDetail");
                    var trans = await _transactionService.InsertTransactionWithDetail(obj, true);
                    AddLogElapsed("MapReturn");
                    rt = ObjectMapper.Map<DepositWalletOutputDto, DepositByCODOutputDto>(trans);
                    await unitOfWork.SaveChangesAsync();
                    // gọi debit tạm thời gọi api 
                    EnmCompanyType CompanyID = 0;
                    if (Enum.IsDefined(typeof(EnmCompanyType), (byte)(inItem.COD.TransporterCode)))
                    {
                        CompanyID = (EnmCompanyType)inItem.COD.TransporterCode;
                    }
                    else
                    {
                        throw new CustomBusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_ENTITY_INVALID)
                                                              .WithData("TransporterCode", inItem.COD.TransporterCode);
                    }
                    var TransporterName = Enum.GetName(typeof(EnmCompanyType), inItem.COD.TransporterCode);
                    var totalAmount = await _paymentRequestRepository.GetToTalBill(inItem.OrderCode, EnmPaymentRequestStatus.Confirm);
                    var inputDto = new DebitCreateInputDto
                    {
                        OrderCode = inItem.OrderCode,
                        CustomerID = inItem.CustCode,
                        CustomerName = inItem.CustName,
                        TotalAmount = totalAmount != null ? totalAmount.TotalPayment : inItem.COD.Amount,
                        TotalDebitAmount = inItem.COD.Amount,
                        TotalPayment = inItem.Transaction.Amount,
                        CreatedBy = inItem.COD.CreatedBy,
                        ShopCode = inItem.Transaction.ShopCode,
                        CompanyID = CompanyID,
                    };
                    var requestCreateDebit = ObjectMapper.Map<DebitCreateInputDto, FRTTMO.DebitService.Dto.DebitCreateInputDto>(inputDto);
                    await _debitAppService.Create(requestCreateDebit);

                }
                catch (CustomBusinessException ex)
                {
                    await unitOfWork.RollbackAsync();
                    throw ex;
                }
                catch (AbpRemoteCallException ab)
                {
                    await unitOfWork.RollbackAsync();
                    throw ab;
                }
                catch (Exception)
                {
                    await unitOfWork.RollbackAsync();
                    throw;
                }
            }
            await _iPublishService.ProduceAsync(new TransactionCreatedETO
            {
                OrderCode = ord.OrderCode,
                Data = rt
            });
            if (inItem.Transaction.TransactionTypeId == EnmTransactionType.FirstDeposit)
            {
                await _iPublishService.ProduceAsync(new CollectDepositTransactionCreatedETO
                {
                    OrderCode = ord.OrderCode,
                    Data = rt
                });
            }
            return rt;
        }
        public async Task<DepositByTransferOutputDto> DepositByTransfer(DepositByTransferInputDto inItem)
        {
            var obj = ObjectMapper.Map<DepositByTransferInputDto, DepositCoresInputDto>(inItem);
            var ord = await InitBeforDeposit(EnmPaymentMethod.Transfer, obj);
            DepositByTransferOutputDto rt;
            using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true))
            {
                try
                {
                    AddLogElapsed("InsertTransDetail");
                    var trans = await _transactionService.InsertTransactionWithDetail(obj, true);
                    AddLogElapsed("MapReturn");
                    rt = ObjectMapper.Map<DepositWalletOutputDto, DepositByTransferOutputDto>(trans);
                    await unitOfWork.SaveChangesAsync();
                }
                catch (Exception)
                {
                    await unitOfWork.RollbackAsync();
                    throw;
                }
            }
            await _iPublishService.ProduceAsync(new TransactionCreatedETO
            {
                OrderCode = ord.OrderCode,
                Data = rt
            });
            if (inItem.Transaction.TransactionTypeId == EnmTransactionType.FirstDeposit)
            {
                await _iPublishService.ProduceAsync(new CollectDepositTransactionCreatedETO
                {
                    OrderCode = ord.OrderCode,
                    Data = rt
                });
            }
            return rt;
        }
        public async Task<DepositByVoucherOutputDto> DepositByVoucher(DepositByVoucherInputDto inItem)
        {
            var _VoucherUsedSuccess = false; //TODO: xử lý _VoucherUsedSuccess trong ghi log của controller

            var obj = ObjectMapper.Map<DepositByVoucherInputDto, DepositCoresInputDto>(inItem);
            var ord = await InitBeforDeposit(EnmPaymentMethod.Voucher, obj);
            //Insert  transaction
            DepositByVoucherOutputDto rt;
            using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true))
            {
                try
                {
                    AddLogElapsed("InsertTransDetail");
                    var trans = await _transactionService.InsertTransactionWithDetail(obj, true);
                    AddLogElapsed("UseVoucher");
                    //Use Voucher
                    if (obj.Vouchers.Any(c => c.VoucherType == EnmVoucherProvider.Taptap))
                    {
                        var useVoucherTap2 = await _taptapAppService.AddTransaction(new PaymentIntegration.Dto.TapTapAddTransactionInputDto
                        {
                            BillId = ord.OrderCode,
                            CreatedDateTime = inItem.Transaction.CreatedDate.ToString("dd-MM-yyyy HH:mm:ss"),
                            Money = (long)ord.TotalPayment,
                            CustomerId = inItem.CustCode,
                            CustomerName = inItem.CustName,
                            CustomerMobile = inItem.Phone,
                            StoreId = inItem.Transaction.ShopCode,
                            Coupons = obj.Vouchers.Where(c => c.VoucherType == EnmVoucherProvider.Taptap).Select(c => new PaymentIntegration.Dto.TapTapCouponInputDto
                            {
                                Code = c.Code,
                                Value = (long)c.Amount.Value
                            }).ToList()
                        });
                        if (!useVoucherTap2.UseVoucherSuccessed) throw new BusinessException(PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", "Sử dụng Voucher không thành công!").WithData("Data", useVoucherTap2).WithData("Voucher Used Success", _VoucherUsedSuccess);
                    }

                    if (obj.Vouchers.Any(c => c.VoucherType == EnmVoucherProvider.GotIT))
                    {
                        var _GotITPin = "";
                        var srhPin = await _vendorPinService.GetByVendor((int)EnmPartnerId.Gotit, inItem.Transaction.ShopCode);
                        if (srhPin != null) _GotITPin = srhPin.PinCode;
                        var useVoucherGotIT = await _gotITAppService.UseMultipleVoucher(new PaymentIntegration.Dto.GotITUseMultipleVoucherInputDto
                        {
                            Bill_Number = ord.OrderCode,
                            Code = obj.Vouchers.Where(c => c.VoucherType == EnmVoucherProvider.GotIT).Select(c => c.Code).ToList(),
                            Total_Bill = (long)ord.TotalPayment,
                            Pin = _GotITPin
                        });
                        if (!useVoucherGotIT.Success) throw new BusinessException(PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", "Sử dụng Voucher không thành công!").WithData("Data", useVoucherGotIT).WithData("Voucher Used Success", _VoucherUsedSuccess);
                    }

                    if (obj.Vouchers.Any(c => c.VoucherType == EnmVoucherProvider.LC))
                    {
                        foreach (var chil in obj.Vouchers.Where(c => c.VoucherType == EnmVoucherProvider.LC))
                        {
                            var useVoucherLC = await _longChauService.VoucherUse(new PaymentIntegration.Dto.LongChauVoucherUseInputDto
                            {
                                voucherCode = chil.Code,
                                paymentrequestId = obj.Transaction.PaymentRequestId.ToString()
                            });
                            if (!useVoucherLC.UseVoucherSuccessed) throw new BusinessException(PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", "Sử dụng Voucher không thành công!").WithData("Data", useVoucherLC).WithData("Voucher Used Success", _VoucherUsedSuccess);
                        }
                    }
                    if (obj.Vouchers.Any(c => c.VoucherType == EnmVoucherProvider.UTop))
                    {
                        var useVoucherUTop = await _uTopAppService.UseVoucher(new PaymentIntegration.Dto.UTopUseVoucherInputDto
                        {
                            Bill_Number = ord.OrderCode,
                            VoucherCodes = obj.Vouchers.Where(c => c.VoucherType == EnmVoucherProvider.UTop).Select(c => c.Code).ToList()
                        });
                        if (!useVoucherUTop.IsValidate) throw new BusinessException(PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", "Sử dụng Voucher không thành công!").WithData("Data", useVoucherUTop).WithData("Voucher Used Success", _VoucherUsedSuccess);
                    }
                    _VoucherUsedSuccess = true;
                    AddLogElapsed("MapReturn");
                    rt = ObjectMapper.Map<DepositWalletOutputDto, DepositByVoucherOutputDto>(trans);
                    AddLogElapsed("SaveChange");
                    await unitOfWork.SaveChangesAsync();
                    AddLogElapsed("EndSaveChange");
                }
                catch (Exception)
                {
                    await unitOfWork.RollbackAsync();
                    throw;
                }
            }
            rt.VoucherUsedSuccess = _VoucherUsedSuccess;
            await _iPublishService.ProduceAsync(new TransactionCreatedETO
            {
                OrderCode = ord.OrderCode,
                Data = rt
            });
            if (inItem.Transaction.TransactionTypeId == EnmTransactionType.FirstDeposit)
            {
                await _iPublishService.ProduceAsync(new CollectDepositTransactionCreatedETO
                {
                    OrderCode = ord.OrderCode,
                    Data = rt
                });
            }
            return rt;
        }
        public async Task<DepositByEWalletOutputDto> DepositByEWalletOnline(DepositByEWalletOnlineInputDto inItem)
        {
            var obj = ObjectMapper.Map<DepositByEWalletOnlineInputDto, DepositCoresInputDto>(inItem);
            var ord = await InitBeforDeposit(obj.Transaction.PaymentMethodId.Value, obj);
            //Check ví điện tử có hỗ trợ hay ko
            if (!Enum.IsDefined(typeof(EnmWalletProvider), obj.EWallet.TypeWalletId))
                throw new BusinessException(PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", "Ví điện tử này không được hỗ trợ!");
            obj.EWallet.RealAmount = inItem.EWallet.RealAmount;
            //Insert  transaction
            DepositByEWalletOutputDto rt;
            using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true))
            {
                try
                {
                    var trans = await _transactionService.InsertTransactionWithDetail(obj, true);
                    rt = ObjectMapper.Map<DepositWalletOutputDto, DepositByEWalletOutputDto>(trans);
                    await unitOfWork.SaveChangesAsync();
                }
                catch (Exception)
                {
                    await unitOfWork.RollbackAsync();
                    throw;
                }
            }
            await _iPublishService.ProduceAsync(new TransactionCreatedETO
            {
                OrderCode = ord.OrderCode,
                Data = rt
            });
            return rt;
        }
        public async Task<DepositByMultipleVoucherOutputDto> DepositByMultipleVoucher(DepositByVoucherInputDto inItem)
        {
            var rt = new DepositByMultipleVoucherOutputDto
            {
                //Untreated = new List<PaymentVoucherInputDto>(),
                Succeeded = new List<DepositByVoucherOutputDto>(),
                Failed = new List<VoucherFailOutputDto>()
            };
            var AmountTransInp = inItem.Transaction.Amount;
            var _AmountDeposited = 0m;
            try
            {
                var chkPayRQ = await _paymentRequestRepository.GetById(inItem.Transaction.PaymentRequestId.Value);
                if (chkPayRQ == null) throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_REQUEST_NOTFOUND)
                                                              .WithData(PaymentCoreErrorMessageKey.OrderCode, inItem.OrderCode)
                                                              .WithData(PaymentCoreErrorMessageKey.PaymentRequestId, inItem.Transaction.PaymentRequestId);
                if ((chkPayRQ.Status == null) || (chkPayRQ.Status != EnmPaymentRequestStatus.Confirm)) throw new BusinessException(PaymentCoreErrorCodes.ERROR_PAYMENT_PAYMENTREQUEST_STATUS)
                                                                .WithData(PaymentCoreErrorMessageKey.OrderCode, inItem.OrderCode)
                                                                .WithData("Status", chkPayRQ?.Status)
                                                                .WithData(PaymentCoreErrorMessageKey.PaymentRequestId, inItem.Transaction.PaymentRequestId);
                inItem.Transaction.PaymentRequestDate = chkPayRQ.PaymentRequestDate;
                inItem.Transaction.PaymentRequestCode = chkPayRQ.PaymentRequestCode;
                foreach (var vc in inItem.Vouchers)
                {
                    try
                    {
                        if (_AmountDeposited >= AmountTransInp) break;
                        var depsted = await _transactionService.GetTotalDeposited(inItem.Transaction.PaymentRequestId.Value, inItem.Transaction.PaymentRequestDate);
                        if (depsted >= chkPayRQ.TotalPayment) break;

                        var depVC = new DepositByVoucherInputDto
                        {
                            OrderCode = inItem.OrderCode,
                            Vouchers = new List<PaymentVoucherInputDto> {
                                ObjectMapper.Map<PaymentVoucherInputDto, PaymentVoucherInputDto>(vc)
                            },
                            Transaction = ObjectMapper.Map<InsertTransactionInputDto, InsertTransactionInputDto>(inItem.Transaction)
                        };
                        depVC.Transaction.Amount = AmountTransInp - _AmountDeposited;
                        if (depVC.Transaction.Amount > depVC.Vouchers[0].Amount) depVC.Transaction.Amount = depVC.Vouchers[0].Amount;

                        var repDepst = await DepositByVoucher(depVC);
                        _AmountDeposited += repDepst.Transaction.Amount.Value;
                        rt.Succeeded.Add(repDepst);
                    }
                    catch (Exception ex)
                    {
                        _log.LogError($"{_CoreName}.DepositByMultipleVoucher: OrderCode {inItem.OrderCode}-Voucher {vc.Code}: {ex}");
                        rt.Failed.Add(new VoucherFailOutputDto
                        {
                            Code = vc.Code,
                            VoucherType = vc.VoucherType,
                            ErrorMessage = ex.Message,
                            ErrorCode = (ex is IHasErrorCode) ? (ex as IHasErrorCode).Code : null
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError($"{_CoreName}.DepositByMultipleVoucher|OrderCode {inItem?.OrderCode}| {ex} ");
                rt.Message = ex.Message;
            }
            return rt;
        }

        /// <summary>
        /// deposit cho phần bán nợ
        /// </summary>
        /// <param name="inItem"></param>
        /// <returns></returns>
        public async Task<DepositDebtSaleOutputDto> DepositDebtSale(DepositDebtSaleInputDto inItem)
        {
            try
            {
                var obj = ObjectMapper.Map<DepositDebtSaleInputDto, DepositCoresInputDto>(inItem);
                var ord = await InitBeforDeposit(EnmPaymentMethod.DebtSale, obj);
                //Insert  transaction
                DepositDebtSaleOutputDto rt;
                using (var unitOfWork = _unitOfWorkManager.Begin(isTransactional: true))
                {
                    try
                    {
                        AddLogElapsed("InsertTransDetail");
                        obj.Debit.CustCode = Guid.Parse(inItem.CustCode);
                        obj.Debit.CustName = inItem.CustName;
                        obj.Debit.Phone = inItem.Phone;
                        var trans = await _transactionService.InsertTransactionWithDetail(obj, true);
                        AddLogElapsed("MapReturn");
                        rt = ObjectMapper.Map<DepositWalletOutputDto, DepositDebtSaleOutputDto>(trans);
                        await unitOfWork.SaveChangesAsync();
                        // gọi debit tạm thời gọi api 

                        var totalAmount = await _paymentRequestRepository.GetToTalBill(inItem.OrderCode, EnmPaymentRequestStatus.Confirm);

                        DebitCreateInputDto inputDto = new DebitCreateInputDto()
                        {
                            OrderCode = inItem.OrderCode,
                            CustomerID = inItem.CustCode,
                            CustomerName = inItem.CustName,
                            TotalAmount = totalAmount != null ? totalAmount.TotalPayment : inItem.Debit.Amount,
                            TotalDebitAmount = inItem.Debit.Amount,
                            TotalPayment = inItem.Debit.Amount,
                            CreatedBy = inItem.Debit.CreatedBy,
                            ShopCode = inItem.Transaction.ShopCode,
                            CompanyID = EnmCompanyType.CusDebit,
                            Phone = inItem.Phone,
                            TaxCode = inItem.Debit.TaxCode
                        };
                        var requestCreateDebit = ObjectMapper.Map<DebitCreateInputDto, FRTTMO.DebitService.Dto.DebitCreateInputDto>(inputDto);
                        await _debitAppService.Create(requestCreateDebit);

                        await _iPublishService.ProduceAsync(new TransactionCreatedETO
                        {
                            OrderCode = ord.OrderCode,
                            Data = rt
                        });
                        if (inItem.Transaction.TransactionTypeId == EnmTransactionType.Recharge)
                        {
                            await _iPublishService.ProduceAsync(new CollectDepositTransactionCreatedETO
                            {
                                OrderCode = ord.OrderCode,
                                Data = rt
                            });
                        }
                    }
                    catch (Exception)
                    {
                        await unitOfWork.RollbackAsync();
                        throw;
                    }
                }
                return rt;
            }
            catch (BusinessException ex)
            {
                throw ex;
            }
            catch (AbpRemoteCallException ab)
            {
                throw ab;
            }
            catch (Exception ex)
            {
                _log.LogError($"{_CoreName}.DepositDebtSale OrderCode {inItem?.OrderCode}: {ex}");
                if (ex is AbpDbConcurrencyException)
                    throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_CONCURENCY_EXCEPTION)
                                .WithData("Message", $"Order {inItem?.OrderCode}: error - " + ex.Message);
                if ((ex is AbpRemoteCallException) || (ex is BusinessException) || (ex is UserFriendlyException))
                    throw;
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION)
                    .WithData(PaymentCoreErrorMessageKey.OrderCode, inItem?.OrderCode)
                    .WithData(PaymentCoreErrorMessageKey.Message, ex.Message);
            }
        }
    }
}
