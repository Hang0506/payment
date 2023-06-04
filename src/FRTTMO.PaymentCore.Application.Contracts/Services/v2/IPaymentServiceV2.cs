using FRTTMO.PaymentCore.Application.Contracts;
using FRTTMO.PaymentCore.Dto.v2;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Services.v2
{
    public interface IPaymentServiceV2 : IPaymentCoreAppServiceBase
    {
        Task<PaymentInfoOutputDtoV2> GetPaymentInfoByPaymentRequest(PaymentInfoInputDtoV2 paymentInfoInput);
        Task<CreatePaymentTransactionOutputDtoV2> CreatePaymentRequest(CreatePaymentTransactionInputDtoV2 inputDto);
        Task<bool> CheckExists(string paymentRequestCode);
        Task<PaymentRequestFullOutputDtoV2> GetByPaymentRequestCode(string paymentRequestCode);
        Task<PaymentDepositInfoOutputDtoV2> GetPaymentDepositInfoByPaymentRequest(PaymentDepositInfoInputDtoV2 inputDto);
        Task<CreatePaymentTransactionOutputDtoV2> CreateWithdrawDepositCash(CreateWithdrawDepositInputDtoV2 inputDto);
        Task<CreateWithdrawDepositTransferOutputDtoV2> CreateWithdrawDepositTransfer(CreateWithdrawDepositTransferInputDtoV2 inputDto);
        Task<CreatePaymentOutputDto> CreatePayment(CreatePaymentInputDto inputDto);
        Task<PaymentRequestFullOutputDtoV2> GetByPaymentCode(string paymentCode);
        Task<TransferFullOutputDtoV2> GetTranferByPaymentCode(string paymentCode);
        Task<OutPutPaymentDtoV2> GetListPaymentCodeByDateTime(InputPaymentDtoV2 input);
        Task<bool> InsertPaymentSource(PaymentSourcDto insertInput);
        Task<TransferUpdateOutDto> UpdateTransfer(TransferUpdateInputDto inputDto);
    }
    public interface IDepositServiceV2 : IPaymentCoreAppServiceBase
    {
        Task<DepositByCashOutputDtoV2> DepositByCash(MaskDepositByCashInputDtoV2 inItem);
        Task<DepositByEWalletOutputDtoV2> DepositByEWallet(MaskDepositByEWalletInputDtoV2 inItem);
        Task<DepositByCardOutputDtoV2> DepositByCard(MaskDepositByCardInputDtoV2 inItem);
        Task<DepositByCODOutputDtoV2> DepositByCOD(MaskDepositByCODInputDtoV2 inItem);
        Task<DepositByTransferOutputDtoV2> DepositByTransfer(MaskDepositByTransferInputDtoV2 inItem);
        Task<DepositByVoucherOutputDtoV2> DepositByVoucher(MaskDepositByVoucherInputDtoV2 inItem);
        Task<DepositByEWalletOutputDtoV2> DepositByEWalletOnline(MaskDepositByEWalletOnlineInputDtoV2 inItem);
        Task<DepositByMultipleVoucherOutputDtoV2> DepositByMultipleVoucher(MaskDepositByVoucherInputDtoV2 inItem);
        Task<DebtSaleFullOutputV2Dto> DepositDebtSaleAll(DepositAllInputDto inItem);
        Task<VerifyTDTOutputDto> FinishTSTD(VerifyTDTSInputDto inItem);
        Task<CreateRequestDepositAllOutputDto> CreateRequestDepositAll(CreateRequestDepositAllInputDto inItem);
        Task<CodFullOutputDtoV2Dto> DepositCodsAll(CodRequestDto requestDto);
        Task<CashFullOutputV2Dto> DepositCashAll(DepositAllInputDto inItem);

        Task<VoucherFullOutputV2Dto> DepositVoucherAll(VoucherRequestDto vouchers);
        Task<EWalletDepositFullOutputV2Dto> DepositEWalletAll(eWalletRequestDto eWallet);
        Task<EWalletDepositFullOutputV2Dto> DepositEWalletOnlineAll(eWalletRequestDto eWallet);
        Task<TransferFullOutputV2Dto> DepositTransferAll(TransferRequestDto tranfer);
        Task<CardFullOutputV2Dto> DepositCardsAll(CardRequestDto cards);
        Task<DepositAllOutDto> DepositSyntheticAll(DepositAllInputDto inItem);
        Task<bool> MapDataSyncES(MapESDepositDto response);
        Task<MigrationPaymentrequestCodeOutnputDto> MigrationPaymentRequestCode(MigrationPaymentrequestCodeInnputDto inputMigration);
        Task<VoucherFullOutputV2Dto> VerifyVoucher(VoucherRequestDto request);
    }
}
