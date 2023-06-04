using AutoMapper;
using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Dto.v2;
using FRTTMO.PaymentCore.Entities;
using FRTTMO.PaymentCore.Kafka.Eto;
using FRTTMO.PaymentCore.Options;
using Newtonsoft.Json;
using System;
using Volo.Abp.Http;
using static FRTTMO.PaymentCore.Dto.CreateQrCodeDto;

namespace FRTTMO.PaymentCore.AutoMapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            #region Customer
            CreateMap<CustomerInsertInputDto, Customer>()
                .ForMember(dts => dts.CreatedDate, opts => opts.MapFrom(src => DateTime.Now))
                .ForMember(dts => dts.ModifiedDate, opts => opts.Ignore())
                .ForMember(dts => dts.ModifiedBy, opts => opts.Ignore())
                ;
            CreateMap<CustomerUpdateInputDto, Customer>()
                .ForMember(dts => dts.ModifiedDate, opts => opts.MapFrom(src => DateTime.Now))
                .ForMember(dts => dts.CreatedBy, opts => opts.Ignore())
                ;
            CreateMap<Customer, CustomerFullOutputDto>()
                ;
            CreateMap<CustomerInsertInputDto, VerifyCustomerRequestDto>()
                ;
            #endregion

            #region Payment Info
            CreateMap<PaymentRequest, PaymentInfoOutputDto>();
            CreateMap<Transaction, TransactionFullOutputDto>();
            CreateMap<Voucher, VoucherFullOutputDto>();
            CreateMap<VoucherFullOutputDto, VoucherFullOutputDto>();
            CreateMap<Card, CardFullOutputDto>();
            CreateMap<CardFullOutputDto, CardFullOutputDto>();
            CreateMap<COD, CODFullOutputDto>();
            CreateMap<CODFullOutputDto, CODFullOutputDto>();
            CreateMap<Transfer, TransferFullOutputDto>();
            CreateMap<TransferFullOutputDto, TransferFullOutputDto>();
            CreateMap<EWalletDeposit, EWalletDepositFullOutputDto>();
            CreateMap<EWalletDepositFullOutputDto, EWalletDepositFullOutputDto>();
            CreateMap<TransactionFullOutputDto, TransactionFullOutputDto>();
            CreateMap<TransactionFullOutputDto, PaymentInfoDetailDto>()
                .ForMember(dst => dst.Detail, opts => opts.MapFrom(src => new TransactionFullOutputDetailDto()));
            CreateMap<VoucherInputDto, Voucher>();
            CreateMap<InsertTransactionInputDto, Transaction>();

            CreateMap<PaymentRequestInsertDto, PaymentRequest>();
            CreateMap<PaymentRequest, PaymentRequestFullOutputDto>();
            CreateMap<PaymentRequest, PaymentRequestCreatedETO>();
            CreateMap<CardInputDto, Card>();
            CreateMap<CODInputDto, COD>();
            CreateMap<TransferInputDto, Transfer>();
            CreateMap<EWalletDepositInputDto, EWalletDeposit>();
            CreateMap<AccountUpdateInputDto, Account>();
            CreateMap<AccountInsertInputDto, Account>()
                .ForMember(dts => dts.CreatedDate, opts => opts.MapFrom(src => DateTime.Now))
                .ForMember(dts => dts.AccountNumber, opts => opts.MapFrom(src => Guid.NewGuid()));
            CreateMap<Account, AccountFullOutputDto>();
            CreateMap<CreatePaymentTransactionInputDto, DepositCoresInputDto>();
            CreateMap<CreatePaymentTransactionOutputDto, DepositWalletOutputDto>();
            CreateMap<DepositWalletOutputDto, CreatePaymentTransactionOutputDto>();
            CreateMap<CardFullOutputDto, CardFullOutputEto>();
            CreateMap<EWalletDepositFullOutputDto, EWalletDepositFullOutputEto>();
            CreateMap<VoucherFullOutputDto, VoucherFullOutputEto>();
            CreateMap<CreatePaymentTransactionInputDto, InsertTransactionInputDto>();
            CreateMap<PaymentCardInputDto, CardInputDto>();
            CreateMap<PaymentCODInputDto, CODInputDto>();
            CreateMap<PaymentTransferInputDto, TransferInputDto>();
            CreateMap<PaymentEWalletDepositInputDto, EWalletDepositInputDto>();
            CreateMap<PaymentVoucherInputDto, VoucherInputDto>();
            CreateMap<PaymentVoucherInputDto, PaymentVoucherInputDto>();
            CreateMap<PaymentTransactionInputDto, InsertTransactionInputDto>();
            CreateMap<CreatePaymentTransactionOutputDto, PaymentTransactionCompletedOutputEto>();
            CreateMap<CreatePaymentTransactionOutputDto, PaymentRequestCompletedOutputEto>();
            CreateMap<CreatePaymentTransactionInputDto, PaymentRequestFailedOutputEto>();
            CreateMap<PaymentTransactionInputDto, PaymentTransactionInputEto>();
            CreateMap<PaymentMethod, PaymentMethodDto>()
                .ForMember(dts => dts.Vendors, opts => opts.MapFrom(src => src.Vendors));
            CreateMap<Vendor, VendorDto>()
                .ForMember(dts => dts.DomesticOption, opts => opts.MapFrom(src => JsonConvert.DeserializeObject<DomesticOption>(src.Domestic)));
            CreateMap<VendorPin, VendorPinDto>().ReverseMap();
            CreateMap<VendorPin, VendorPinFullOutputDto>();
            CreateMap<InsertVenderInputDto, VendorPin>();
            CreateMap<UpdateVenderInputDto, VendorPin>();
            CreateMap<PaymentRequestCompletedOutputDto, PaymentRequestCompletedOutputEto>();
            CreateMap<PaymentRequestCompletedOutputDto, PaymentRequestCompletedDebbitOutputEto>();
            CreateMap<PaymentRequestCompletedOutputDto, PaymentRequestCompletedAROutputEto>()
                .ForMember(dst => dst.DepositTransactions, opts => opts.Ignore());
            CreateMap<PaymentInfoDetailDto, PaymentInfoDetailEto>();
            CreateMap<TransactionFullOutputDetailDto, TransactionFullOutputDetailEto>();
            CreateMap<CODFullOutputDto, CODFullOutputEto>();
            CreateMap<TransferFullOutputDto, TransferFullOutputEto>();
            #endregion

            //VNPay
            CreateMap<VNPayCheckOrderResultEntity, VNPayCheckOrderOutputDto>(MemberList.Source)
                .ForMember(dts => dts.Message, opts => opts.MapFrom(src => SmartPayEnvironment.ErrorCodes.ContainsKey(src.Code) ? SmartPayEnvironment.ErrorCodes[src.Code] : null))
                ;

            //SmartPay
            CreateMap<SmartpayCheckOrderResultEntity, SmartpayCheckOrderOutputDto>(MemberList.Source)
                 .ForMember(dts => dts.Data, opts => opts.MapFrom(src => src.Data))
                ;
            CreateMap<SmartpayCheckOrderResultDetail, SmartpayCheckOrderResultDetailDto>(MemberList.Source)
                ;
            CreateMap<CreateQrCodeInputDto, CreateQrCodeVNPayInputDto>()
                .ForMember(dst => dst.serviceCode, opts => opts.MapFrom(src => src.ServiceCode))
                .ForMember(dst => dst.countryCode, opts => opts.MapFrom(src => src.CountryCode))
                .ForMember(dst => dst.payType, opts => opts.MapFrom(src => src.PayType))
                .ForMember(dst => dst.productId, opts => opts.MapFrom(src => src.ProductId))
                .ForMember(dst => dst.txnId, opts => opts.MapFrom(src => src.TxnId))
                .ForMember(dst => dst.billNumber, opts => opts.MapFrom(src => src.BillNumber))
                .ForMember(dst => dst.amount, opts => opts.MapFrom(src => src.Amount.ToString()))
                .ForMember(dst => dst.ccy, opts => opts.MapFrom(src => src.Ccy))
                .ForMember(dst => dst.expDate, opts => opts.MapFrom(src => src.ExpDate.HasValue ? src.ExpDate.Value.ToString("yyMMddHHmm") : null))
                .ForMember(dst => dst.desc, opts => opts.MapFrom(src => src.Desc))
                .ForMember(dst => dst.masterMerCode, opts => opts.MapFrom(src => src.MasterMerCode))
                .ForMember(dst => dst.tipAndFee, opts => opts.MapFrom(src => src.TipAndFee))
                .ForMember(dst => dst.consumerID, opts => opts.MapFrom(src => src.ConsumerID))
                .ForMember(dst => dst.purpose, opts => opts.MapFrom(src => src.Purpose))
                .ForMember(dst => dst.terminalId, opts => opts.MapFrom(src => src.ShopCode));
            CreateMap<CreateQrCodeVNPayOutputDto, CreateQrCodeOutputDto>()
                .ForMember(dst => dst.QrCode, opts => opts.MapFrom(src => src.Data));

            CreateMap<CreateQrCodeInputDto, CreateQrCodeSmartpayInputDto>()
                .ForMember(dst => dst.subChannel, opts => opts.MapFrom(src => src.SubChannel))
                .ForMember(dst => dst.desc, opts => opts.MapFrom(src => src.Desc))
                .ForMember(dst => dst.orderNo, opts => opts.MapFrom(src => src.OrderNo))
                .ForMember(dst => dst.extras, opts => opts.MapFrom(src => src.Extras))
                .ForMember(dst => dst.amount, opts => opts.MapFrom(src => Convert.ToInt64(src.Amount)))
                .ForMember(dst => dst.created, opts => opts.MapFrom(src => src.Created.HasValue ? src.Created.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null));
            CreateMap<CreateQrCodeSmartpayOutputDto, CreateQrCodeOutputDto>()
                .ForMember(dst => dst.QrCode, opts => opts.MapFrom(src => src.Data.Payload));

            //Deposit
            CreateMap<InsertTransactionInputDto, InsertTransactionInputDto>();
            CreateMap<EWalletDepositInputDto, EWalletDepositInputDto>();
            CreateMap<CardInputDto, CardInputDto>();
            CreateMap<CODInputDto, CODInputDto>();
            CreateMap<TransferInputDto, TransferInputDto>();
            CreateMap<VoucherInputDto, VoucherInputDto>();
            CreateMap<DepositByCashInputDto, DepositCoresInputDto>(MemberList.Source)
                 .ForMember(dts => dts.Transaction, opts => opts.MapFrom(src => src.Transaction))
                 ;
            CreateMap<DepositByEWalletInputDto, DepositCoresInputDto>(MemberList.Source)
                 .ForMember(dts => dts.Transaction, opts => opts.MapFrom(src => src.Transaction))
                 .ForMember(dts => dts.EWallet, opts => opts.MapFrom(src => src.EWallet))
                 ;
            CreateMap<DepositByCardInputDto, DepositCoresInputDto>(MemberList.Source)
                 .ForMember(dts => dts.Transaction, opts => opts.MapFrom(src => src.Transaction))
                 .ForMember(dts => dts.Cards, opts => opts.MapFrom(src => src.Cards))
                 ;
            CreateMap<DepositByCODInputDto, DepositCoresInputDto>(MemberList.Source)
                 .ForMember(dts => dts.Transaction, opts => opts.MapFrom(src => src.Transaction))
                 .ForMember(dts => dts.COD, opts => opts.MapFrom(src => src.COD))
                 ;
            CreateMap<DepositByTransferInputDto, DepositCoresInputDto>(MemberList.Source)
                 .ForMember(dts => dts.Transaction, opts => opts.MapFrom(src => src.Transaction))
                 .ForMember(dts => dts.Transfers, opts => opts.MapFrom(src => src.Transfers))
                 ;
            CreateMap<DepositByVoucherInputDto, DepositCoresInputDto>(MemberList.Source)
                 .ForMember(dts => dts.Transaction, opts => opts.MapFrom(src => src.Transaction))
                 .ForMember(dts => dts.Vouchers, opts => opts.MapFrom(src => src.Vouchers))
                 ;

            //Deposit Output
            CreateMap<DepositWalletOutputDto, DepositByCashOutputDto>(MemberList.Source)
                 .ForMember(dts => dts.Transaction, opts => opts.MapFrom(src => src.Transaction))
                ;
            CreateMap<DepositWalletOutputDto, DepositByEWalletOutputDto>(MemberList.Source)
                 .ForMember(dts => dts.Transaction, opts => opts.MapFrom(src => src.Transaction))
                 .ForMember(dts => dts.EWallet, opts => opts.MapFrom(src => src.EWallet))
                ;
            CreateMap<DepositWalletOutputDto, DepositByCardOutputDto>(MemberList.Source)
                 .ForMember(dts => dts.Transaction, opts => opts.MapFrom(src => src.Transaction))
                 .ForMember(dts => dts.Cards, opts => opts.MapFrom(src => src.Cards))
                ;
            CreateMap<DepositWalletOutputDto, DepositByCODOutputDto>(MemberList.Source)
                 .ForMember(dts => dts.Transaction, opts => opts.MapFrom(src => src.Transaction))
                 .ForMember(dts => dts.COD, opts => opts.MapFrom(src => src.COD))
                ;
            CreateMap<DepositWalletOutputDto, DepositByTransferOutputDto>(MemberList.Source)
                 .ForMember(dts => dts.Transaction, opts => opts.MapFrom(src => src.Transaction))
                 .ForMember(dts => dts.Transfers, opts => opts.MapFrom(src => src.Transfers))
                ;
            CreateMap<DepositWalletOutputDto, DepositByVoucherOutputDto>(MemberList.Source)
                 .ForMember(dts => dts.Transaction, opts => opts.MapFrom(src => src.Transaction))
                 .ForMember(dts => dts.Vouchers, opts => opts.MapFrom(src => src.Vouchers))
                ;
            CreateMap<LogApiDto, LogApi>();
            CreateMap<LogApiDto, LogApiES>();
            CreateMap<TransactionFullOutputDto, TransactionFullOutputEto>();

            // Refund
            CreateMap<RefundDto, Refund>()
                .ForMember(dts => dts.CreatedDate, opts => opts.MapFrom(src => DateTime.Now))
                .ForMember(dts => dts.ModifiedDate, opts => opts.Ignore())
                .ForMember(dts => dts.ModifiedBy, opts => opts.Ignore())
                ;
            CreateMap<Refund, RefundFullOutputDto>()
                ;
            //
            CreateMap<RefundDto, InsertTransactionInputDto>()
                .ForMember(dts => dts.AccountId, opts => opts.MapFrom(src => src.CustomerId)) // CustomerId tương đương với AccountId
                .ForMember(dts => dts.CreatedDate, opts => opts.MapFrom(src => DateTime.Now))
                ;
            CreateMap<TransactionFullOutputDto, RefundFullOutputDto>()
                .ForMember(dst => dst.CustomerId, opts => opts.MapFrom(src => src.AccountId)); // CustomerId tương đương với AccountId

            // Bank
            CreateMap<Bank, BankFullOutputDto>();
            // Card Type
            CreateMap<CardType, CardTypeFullOutputDto>();
            // BankAccount
            CreateMap<BankAccount, BankAccountFullOutputDto>();
            //
            CreateMap<PaymentRequest, PaymentDepositInfoOutputDto>();
            CreateMap<PaymentRequest, PaymentDepositRequestIdInfoOutputDto>();
            CreateMap<BankCard, BankCardFullOutputDto>();
            CreateMap<DepositByEWalletOnlineInputDto, DepositCoresInputDto>();


            //thu coc, chi coc
            CreateMap<CreateWithdrawDepositInputDto, PaymentRequestFailedOutputEto>();
            CreateMap<CreateWithdrawDepositTransferInputDto, PaymentRequestFailedOutputEto>();
            CreateMap<CreateWithdrawDepositTransferOutputDto, WithdrawDepositCompletedOutputEto>();
            CreateMap<CreatePaymentTransactionOutputDto, WithdrawDepositCompletedOutputEto>();

            //hủy cọc
            //get hinhf thức chi tiền
            CreateMap<PaymentMethod, PaymentPayMethodDto>();
            CreateMap<PaymentMethodDetail, PaymentMethodDetailDto>();
            CreateMap<CancelDepositDto, PaymentRequestInsertDto>();
            CreateMap<CancelDepositDto, CashbackDepositRefundBaseDto>();
            CreateMap<InsertTransactionCancelInputDto, InsertTransactionInputDto>();

            CreateMap<PaymentIntegration.Dto.BankListDto, VendorDto>()
               .ForMember(dst => dst.VendorCode, opts => opts.MapFrom(src => src.bank_code))
               .ForMember(dst => dst.VendorName, opts => opts.MapFrom(src => src.bank_name))
               .ForMember(dst => dst.ImageUrl, opts => opts.MapFrom(src => src.logo_link))
               .ForMember(dst => dst.PaymentMethodId, opts => opts.MapFrom(src => src.bank_type));
            CreateMap<BankAccountFull, BankAccountFullOutputDto>();
            CreateMap<TransactionReturnDto, InsertTransactionInputDto>();
            CreateMap<TransactionFullOutputDto, InsertTransactionInputDto>();
            CreateMap<PaymentRequestTransferDto, PaymentRequestInsertDto>();
            //sync es v2
            CreateMap<PaymentCODInputDtoV2, CODFullOutputDto>();
            CreateMap<CODFullOutputDto, CODFullOutputDto>();

            // chi tiền trả hàng
            CreateMap<CreatePaymentTransactionOutputDto, WithdrawReturnCompletedOutputEto>();
            CreateMap<TransferFullOutputDto, TransferFullOutputEto>();

            //bán nợ
            CreateMap<DepositDebtSaleInputDto, DepositCoresInputDto>();
            CreateMap<DepositWalletOutputDto, DepositDebtSaleOutputDto>();
            ///
            /// ESlatiscSearch
            CreateMap<COD, CODDetail>();
            CreateMap<EWalletDeposit, EWalletDepositDetail>();
            CreateMap<Voucher, VoucherDetail>();
            CreateMap<Transfer, TransferDetail>();
            CreateMap<Card, CardDetail>();
            CreateMap<TransactionDetailPaymentMethod, Dto.TransactionDetailDto.TransactionDetailOutputDto>();
            CreateMap<Dto.TransactionDetailDto.TransactionDetailOutputDto, TransactionIndex>();


            //
            CreateMap<DebitDto, CreditSales>().ForMember(dst => dst.CustomerId, opts => opts.MapFrom(src => src.CustCode));
            CreateMap<CreditSales, DebitFullOutputDto>().ForMember(dst => dst.CustCode, opts => opts.MapFrom(src => src.CustomerId));

            // VendorDetail
            CreateMap<VendorDetail, VendorDetailDto>();
            // update company
            CreateMap<COD, UpdateCodCompanyEto>();
            CreateMap<DebitCreateInputDto, FRTTMO.DebitService.Dto.DebitCreateInputDto>();
            CreateMap<FRTMO.DebitService.Dtos.AccountingHistoryDetailOutputDto, AccountingHistoryDetailOutputDto>();
        }
    }
}
