using AutoMapper;
using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Dto.TransactionDetailDto;
using FRTTMO.PaymentCore.Dto.v2;
using FRTTMO.PaymentCore.Entities;
using FRTTMO.PaymentCore.Kafka.Eto;
using FRTTMO.PaymentCore.Options;
using System;

namespace FRTTMO.PaymentCore.AutoMapper
{
    public class AutoMapperProfileV2 : Profile
    {
        public AutoMapperProfileV2()
        {
            #region Payment

            CreateMap<CreatePaymentTransactionInputDtoV2, PaymentRequestFailedOutputEto>();
            CreateMap<PaymentTransactionInputDtoV2, PaymentTransactionInputEto>();
            CreateMap<PaymentTransactionInputDtoV2, InsertTransactionInputDto>();
            CreateMap<PaymentTransactionInputDtoV2, InsertTransactionInputDtoV2>();
            CreateMap<CreatePaymentTransactionOutputDtoV2, PaymentTransactionCompletedOutputEto>();
            CreateMap<TransactionFullOutputDto, TransactionFullOutputDtoV2>();
            CreateMap<InsertTransactionInputDtoV2, Transaction>();
            CreateMap<Transaction, TransactionFullOutputDtoV2>();
            CreateMap<CreatePaymentTransactionOutputDtoV2, WithdrawDepositCompletedOutputEto>();
            CreateMap<CreateWithdrawDepositTransferOutputDtoV2, WithdrawDepositCompletedOutputEto>();
            CreateMap<TransactionFullOutputDtoV2, TransactionFullOutputEto>();
            CreateMap<PaymentRequest, PaymentRequestFullOutputDtoV2>();
            CreateMap<PaymentRequest, PaymentDepositInfoOutputDtoV2>();
            CreateMap<TransactionFullOutputDtoV2, PaymentInfoDetailDtoV2>();
            CreateMap<DepositTransactionInputDtoV2, DepositTransactionInputDtoV2>();

            CreateMap<DepositTransactionInputDtoV2, InsertTransactionInputDtoV2>()
                .ForMember(dts => dts.CreatedDate, opts => opts.MapFrom(src => DateTime.Now))
                ;
            CreateMap<PaymentEWalletDepositInputDtoV2, EWalletDepositInputDto>()
                .ForMember(dts => dts.CreatedDate, opts => opts.MapFrom(src => DateTime.Now))
                ;
            CreateMap<PaymentCardInputDtoV2, CardInputDto>()
                .ForMember(dts => dts.CreatedDate, opts => opts.MapFrom(src => DateTime.Now))
                ;
            CreateMap<PaymentTransferInputDtoV2, TransferInputDto>()
                .ForMember(dts => dts.CreatedDate, opts => opts.MapFrom(src => DateTime.Now))
                ;
            CreateMap<PaymentCODInputDtoV2, CODInputDto>()
                .ForMember(dts => dts.CreatedDate, opts => opts.MapFrom(src => DateTime.Now))
                ;
            CreateMap<PaymentVoucherInputDtoV2, PaymentVoucherInputDtoV2>();
            CreateMap<PaymentVoucherInputDtoV2, VoucherInputDto>()
                .ForMember(dts => dts.CreatedDate, opts => opts.MapFrom(src => DateTime.Now))
                ;
            CreateMap<TransferFullOutputDto, TransferFullOutputDtoV2>();
            CreateMap<MigrationPaymentrequestCodeInnputDto, InputPaymentDtoV2>();
            #region Deposit v2
            //Input
            CreateMap<MaskDepositInputBaseDtoV2, DepositCoresInputDtoV2>()
                 .ForMember(dts => dts.Transaction, opts => opts.MapFrom(src => src.Transaction))
                 ;
            CreateMap<MaskDepositByCashInputDtoV2, DepositCoresInputDtoV2>()
                 .ForMember(dts => dts.Transaction, opts => opts.MapFrom(src => src.Transaction));

            CreateMap<MaskDepositByEWalletInputDtoV2, DepositCoresInputDtoV2>()
                 .ForMember(dts => dts.Transaction, opts => opts.MapFrom(src => src.Transaction))
                 .ForMember(dts => dts.EWallet, opts => opts.MapFrom(src => src.EWallet))
                 .AfterMap((src, dts) =>
                 {
                     if (dts.EWallet != null) dts.EWallet.CreatedBy = src.Transaction.CreatedBy;
                 })
                 ;
            CreateMap<MaskDepositByCardInputDtoV2, DepositCoresInputDtoV2>()
                 .ForMember(dts => dts.Transaction, opts => opts.MapFrom(src => src.Transaction))
                 .ForMember(dts => dts.Cards, opts => opts.MapFrom(src => src.Cards))
                 .AfterMap((src, dts) =>
                 {
                     if (dts.Cards != null) dts.Cards.ForEach(cd => cd.CreatedBy = src.Transaction.CreatedBy);
                 })
                 ;
            CreateMap<MaskDepositByCODInputDtoV2, DepositCoresInputDtoV2>()
                 .ForMember(dts => dts.Transaction, opts => opts.MapFrom(src => src.Transaction))
                 .ForMember(dts => dts.COD, opts => opts.MapFrom(src => src.COD))
                 .AfterMap((src, dts) =>
                 {
                     if (dts.COD != null) dts.COD.CreatedBy = src.Transaction.CreatedBy;
                 })
                 ;
            CreateMap<MaskDepositByTransferInputDtoV2, DepositCoresInputDtoV2>()
                 .ForMember(dts => dts.Transaction, opts => opts.MapFrom(src => src.Transaction))
                 .ForMember(dts => dts.Transfers, opts => opts.MapFrom(src => src.Transfers))
                 .AfterMap((src, dts) =>
                 {
                     if (dts.Transfers != null) dts.Transfers.ForEach(cd => cd.CreatedBy = src.Transaction.CreatedBy);
                 })
                 ;
            CreateMap<MaskDepositByVoucherInputDtoV2, DepositCoresInputDtoV2>()
                 .ForMember(dts => dts.Transaction, opts => opts.MapFrom(src => src.Transaction))
                 .ForMember(dts => dts.Vouchers, opts => opts.MapFrom(src => src.Vouchers))
                 .AfterMap((src, dts) =>
                 {
                     if (dts.Vouchers != null) dts.Vouchers.ForEach(cd => cd.CreatedBy = src.Transaction.CreatedBy);
                 })
                 ;
            CreateMap<MaskDepositByEWalletOnlineInputDtoV2, DepositCoresInputDtoV2>()
                 .ForMember(dts => dts.Transaction, opts => opts.MapFrom(src => src.Transaction))
                 .ForMember(dts => dts.EWallet, opts => opts.MapFrom(src => src.EWallet))
                 .AfterMap((src, dts) =>
                 {
                     if (dts.EWallet != null)
                     {
                         dts.EWallet.CreatedBy = src.Transaction.CreatedBy;
                     }
                 })
                 ;
            //Deposit Output
            CreateMap<DepositCoresOutputDtoV2, DepositByCashOutputDtoV2>(MemberList.Source)
                 .ForMember(dts => dts.Transaction, opts => opts.MapFrom(src => src.Transaction))
                ;
            CreateMap<DepositCoresOutputDtoV2, DepositByEWalletOutputDtoV2>(MemberList.Source)
                 .ForMember(dts => dts.Transaction, opts => opts.MapFrom(src => src.Transaction))
                 .ForMember(dts => dts.EWallet, opts => opts.MapFrom(src => src.EWallet))
                ;
            CreateMap<DepositCoresOutputDtoV2, DepositByCardOutputDtoV2>(MemberList.Source)
                 .ForMember(dts => dts.Transaction, opts => opts.MapFrom(src => src.Transaction))
                 .ForMember(dts => dts.Cards, opts => opts.MapFrom(src => src.Cards))
                ;
            CreateMap<DepositCoresOutputDtoV2, DepositByCODOutputDtoV2>(MemberList.Source)
                 .ForMember(dts => dts.Transaction, opts => opts.MapFrom(src => src.Transaction))
                 .ForMember(dts => dts.COD, opts => opts.MapFrom(src => src.COD))
                ;
            CreateMap<DepositCoresOutputDtoV2, DepositByTransferOutputDtoV2>(MemberList.Source)
                 .ForMember(dts => dts.Transaction, opts => opts.MapFrom(src => src.Transaction))
                 .ForMember(dts => dts.Transfers, opts => opts.MapFrom(src => src.Transfers))
                ;
            CreateMap<DepositCoresOutputDtoV2, DepositByVoucherOutputDtoV2>(MemberList.Source)
                 .ForMember(dts => dts.Transaction, opts => opts.MapFrom(src => src.Transaction))
                 .ForMember(dts => dts.Vouchers, opts => opts.MapFrom(src => src.Vouchers))
                ;
            #endregion
            CreateMap<PaymentTransactionDtoV2, PaymentTransactionInputEto>();
            CreateMap<PaymentRequestCompletedOutputDtoV2, PaymentRequestCompletedDebbitOutputEto>();
            CreateMap<PaymentTransactionDtoV2, InsertTransactionInputDtoV2>();
            CreateMap<PaymentRequestCompletedOutputDtoV2, PaymentRequestCompletedOutputEto>();
            CreateMap<PaymentRequestCompletedOutputDtoV2, PaymentRequestCompletedAROutputEto>();
            CreateMap<PaymentInfoDetailDtoV2, PaymentInfoDetailEto>();

            CreateMap<PaymentVoucherAllDtoV2, PaymentVoucherInputDtoV2>();
            CreateMap<EWalletDepositInputDto, PaymentEWalletDepositInputDtoV2>();
            CreateMap<TransferInputDto, PaymentTransferInputDtoV2>();
            CreateMap<PaymentCardAllAInputDtoV2, PaymentCardInputDtoV2>();
            CreateMap<TransactionFullOutputDtoV2, PaymentInfoDetailEto>();
            CreateMap<TransactionFullOutputDtoV2, TransactionFullOutputEto>();
            #endregion Payment
            #region create-payment
            CreateMap<PaymentTransactionBaseDto, PaymentSource>();
            CreateMap<Payment, CreatePaymentOutputDto>();
            CreateMap<PaymentRequestInputDto, PaymentRequest>();
            CreateMap<PaymentRequest, PaymentRequestOutputDto>();
            CreateMap<Payment, PaymentCreatedEto>();
            CreateMap<PaymentSource, PaymentTransactionCreatedEto>();
            CreateMap<DepositAllInputDto, MaskDepositByCashInputDtoV2>();
            CreateMap<DepositCashInputDto, DepositTransactionInputDtoV2>();
            CreateMap<DepositCashInputDto, DepositTransactionInputDtoV2>();
            CreateMap<TransactionFullOutputDtoV2, DepositCashOutPutDto>();
            CreateMap<DepositCashInputDto, DepositCashOutPutDto>();
            CreateMap<Payment, PaymentRequestFullOutputDtoV2>();
            //deposit cod
            CreateMap<DepositAllInputDto, ModelBaseMapDeposit>();
            CreateMap<CODInputDto, PaymentCODInputDtoV2>();
            CreateMap<DepositTransactionDto, DepositTransactionInputDtoV2>();
            CreateMap<DepositAllInputDto, CodRequestDto>();
            CreateMap<PaymentSource, PaymentSourceOutPutDto>()
                .ForMember(dts => dts.PaymentSourceId, opts => opts.MapFrom(src => src.Id));

            //deposit 
            CreateMap<DepositAllInputDto, CardRequestDto>();
            CreateMap<DepositAllInputDto, TransferRequestDto>();
            CreateMap<DepositAllInputDto, eWalletRequestDto>();
            CreateMap<DepositAllInputDto, VoucherRequestDto>();
            CreateMap<DepositAllInputDto, CodRequestDto>();
            CreateMap<SucceededCash, Cash>();
            CreateMap<DepositCashOutPutDto, Cash>();

            //deposit bán nơ
            CreateMap<DebtSaleInputDto, DepositCoresInputDtoV2>();
            CreateMap<DepositTransactionInputDtoV2, InsertTransactionInputDtoV2>();
            CreateMap<DebtSaleInputV2Dto, DepositDebtSaleOutPutDto>();
            CreateMap<DebitV2Dto, DebitDto>().ReverseMap();
            CreateMap<DebtSaleInputV2Dto, DebtSaleInputDto>();
            CreateMap<DepositCoresOutputDtoV2, DebtSaleOutputDto>();
            CreateMap<TransactionFullOutputDtoV2, DepositTransactionDto>();
            CreateMap<DebtSaleOutputDto, DepositDebtSaleOutPutDto>();
            #endregion

            #region ElasticSearch
            CreateMap<DepositAllDto, Kafka.Eto.v2.DepositAllEto>();
            CreateMap<PaymentSourceId, Kafka.Eto.v2.PaymentSourceDto>();
            CreateMap<PaymentSource, Kafka.Eto.v2.PaymentSourceDto>();
            CreateMap<Detail, Kafka.Eto.v2.Detail>();
            CreateMap<CardsAll, Kafka.Eto.v2.CardsAll>();
            CreateMap<CardsDetail, Kafka.Eto.v2.CardsDetail>();
            CreateMap<Cash, Kafka.Eto.v2.Cash>();
            CreateMap<CodAll, Kafka.Eto.v2.CodAll>();
            CreateMap<CoDetail, Kafka.Eto.v2.CoDetail>();
            CreateMap<Debit, Kafka.Eto.v2.Debit>();
            CreateMap<DebtSaleAll, Kafka.Eto.v2.DebtSaleAll>();
            CreateMap<EWalletAll, Kafka.Eto.v2.EWalletAll>();
            CreateMap<EWalletDetail, Kafka.Eto.v2.EWalletDetail>();
            CreateMap<EWalletOnlineAll, Kafka.Eto.v2.EWalletOnlineAll>();
            CreateMap<TransactionDeposit, Kafka.Eto.v2.TransactionDeposit>();
            CreateMap<TransferDetailDeposit, Kafka.Eto.v2.TransferDetailDeposit>();
            CreateMap<TransfersAll, Kafka.Eto.v2.TransfersAll>();
            CreateMap<VoucherDetailDeposit, Kafka.Eto.v2.VoucherDetailDeposit>();
            CreateMap<VouchersAll, Kafka.Eto.v2.VouchersAll>();
            CreateMap<PaymentDepositInfoOutputDto, DepositAllDto>();
            //
            #region Redis
            CreateMap<DepositAllDto, PaymentRedisDetailDto>().ReverseMap();
            CreateMap<PaymentSourceId, PaymentSourceRedis>().ReverseMap();
            CreateMap<Detail, DetailRedis>().ReverseMap();
            CreateMap<CardsAll, CardsAllRedis>().ReverseMap();
            CreateMap<CardsDetail, CardsDetailRedis>().ReverseMap();
            CreateMap<Cash, CashRedis>().ReverseMap();
            CreateMap<CodAll, CodAllRedis>().ReverseMap();
            CreateMap<CoDetail, CoDetailRedis>().ReverseMap();
            CreateMap<Debit, DebitRedis>().ReverseMap();
            CreateMap<DebtSaleAll, DebtSaleAllRedis>().ReverseMap();
            CreateMap<EWalletAll, EWalletAllRedis>().ReverseMap();
            CreateMap<EWalletDetail, EWalletDetailRedis>().ReverseMap();
            CreateMap<EWalletOnlineAll, EWalletOnlineAllRedis>().ReverseMap();
            CreateMap<TransactionDeposit, TransactionDepositRedis>().ReverseMap();
            CreateMap<TransferDetailDeposit, TransferDetailDepositRedis>().ReverseMap();
            CreateMap<TransfersAll, TransfersAllRedis>().ReverseMap();
            CreateMap<VoucherDetailDeposit, VoucherDetailDepositRedis>().ReverseMap();
            CreateMap<VouchersAll, VouchersAllRedis>().ReverseMap();
            //
            #endregion
            CreateMap<PaymentRequestDetailDto, Detail>();
            CreateMap<Payment, DepositAllDto>();
            CreateMap<PaymentSource, PaymentSourceId>();
            CreateMap<EWalletDepositFullOutputDto, EWalletAll>();
            CreateMap<CardFullOutputDto, CardsAll>();
            CreateMap<VoucherFullOutputDto, VouchersAll>();
            CreateMap<CODFullOutputDto, CodAll>();
            CreateMap<TransferFullOutputDto, TransfersAll>();
            CreateMap<TransactionFullOutputDtoV2, Cash>();
            CreateMap<DebitFullOutputDto, DebtSaleAll>();
            CreateMap<DebitFullOutputDto, TransactionDeposit>();
            CreateMap<DebitFullOutputDto, Debit>();
            CreateMap<Dto.v2.DebtSaleAll, DebtSaleAll>();
            CreateMap<VoucherFullOutputDto, Dto.v2.TransactionDeposit>();
            CreateMap<VoucherFullOutputDto, Dto.v2.VoucherDetailDeposit>();
            CreateMap<CODFullOutputDto, Dto.v2.TransactionDeposit>();
            CreateMap<CODFullOutputDto, Dto.v2.CoDetail>();

            CreateMap<EWalletDepositFullOutputDto, Dto.v2.TransactionDeposit>();
            CreateMap<EWalletDepositFullOutputDto, Dto.v2.EWalletDetail>();
            CreateMap<CardFullOutputDto, Dto.v2.TransactionDeposit>();
            CreateMap<CardFullOutputDto, Dto.v2.CardsDetail>();
            CreateMap<TransferFullOutputDto, Dto.v2.TransactionDeposit>();
            CreateMap<TransferFullOutputDto, Dto.v2.TransferDetailDeposit>();
            CreateMap<PaymentSourcDto, PaymentSource>();
            CreateMap<PaymentSource, PaymentSourcDto>();
            CreateMap<TransactionFullOutputDtoV2, Dto.v2.Cash>().ForMember(dts => dts.TransactionId, opts => opts.MapFrom(src => src.Id));


            //phần map tạo request sync ES
            CreateMap<PaymentSourceOutPutDto, PaymentSourceId>().ForMember(dts => dts.Id, opts => opts.MapFrom(src => src.PaymentSourceId));
            CreateMap<DepositAllOutDto, DepositAllDto>().ForMember(dts => dts.PaymentSource, opts => opts.MapFrom(src => src.PaymentSourceId));
            CreateMap<PaymentSourceOutPutDto, PaymentSource>();
            CreateMap<EWalletDepositAllDto, EWalletAll>();
            CreateMap<EWalletDepositAllDto, EWalletOnlineAll>();
            CreateMap<EWalletDepositInputDto, EWalletDetail>();
            CreateMap<CardDepositAllDto, CardsAll>();
            CreateMap<PaymentCardAllAInputDtoV2, CardsDetail>();
            CreateMap<CODDepositAllDto, CodAll>();
            CreateMap<CODInputDto, CoDetail>();
            CreateMap<TransferInputDepositAllDto, TransfersAll>();
            CreateMap<TransferInputDto, TransferDetailDeposit>();
            CreateMap<VoucherDepositAllDto, VouchersAll>();

            CreateMap<PaymentVoucherAllDtoV2, VoucherDetailDeposit>();
            CreateMap<DepositDebtSaleOutPutDto, DebtSaleAll>();
            CreateMap<DebitV2Dto, Debit>();
            CreateMap<DepositTransactionDto, TransactionDeposit>();
            CreateMap<DepositAllOutDto, MapESDepositDto>();
            #endregion
            #region SyncES-KTHO
            CreateMap<PaymentRequest, PaymentRequestSyncOutPutDto>();
            CreateMap<TransactionDetailTransferOutputDto, TransactionIndex>();
            CreateMap<TransferFullOutputDto, TransferSyncAll>();
            CreateMap<TransactionFullOutputDtoV2, TransferSyncAll>();
            CreateMap<PaymentSourceOutPutDto, SourceCodeSyncES>();

            #endregion
            #region Refund
            CreateMap<TransactionFullOutputDtoV2, TransactionFullOutputEto>();
            CreateMap<TransactionFullOutputDtoV2, InsertTransactionInputDtoV2>(); //tao cashback
            CreateMap<CreatePaymentTransactionOutputDtoV2, WithdrawDepositCompletedOutputEto>();
            CreateMap<TransferInputDto, TransferInputDto>();
            CreateMap<CreatePaymentTransactionOutputDtoV2, WithdrawReturnCompletedOutputEto>();
            CreateMap<TransferFullOutputDto, TransferFullInputDto>();
            CreateMap<TransferFullOutputDto, TransferFullOutputEto>();
            CreateMap<PaymentRequest, PaymentRequestSyncOutPutDto>();
            CreateMap<TransactionFullOutputDtoV2, TransactionTransferDto>();
            CreateMap<TransferFullOutputDto, TransferFullSyncESDto>();
            CreateMap<TransferFullSyncESDto, TransactionDetailTransferOutputDto>();
            #endregion

            #region BankingOnline
            CreateMap<BankingOnline, BankingOnlineOutPutDto>().ReverseMap();
            #endregion
            #region redis
            CreateMap<PaymentRedisDetailDto, PaymentRedisDto>();
            CreateMap<DepositAllDto, PaymentRedisDetailDto>();
            CreateMap<PaymentSourceId, PaymentSourceRedis>();
            CreateMap<CodAll, CodAllRedis>();
            CreateMap<TransactionDeposit, TransactionDepositRedis>();
            CreateMap<CoDetail, CoDetailRedis>();
            #endregion
        }
    }
}
