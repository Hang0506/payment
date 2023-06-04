using FRTTMO.PaymentCore.Application.Contracts;
using FRTTMO.PaymentCore.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Services
{
    public interface IPaymentService : IPaymentCoreAppServiceBase
    {
        Task<PaymentInfoOutputDto> GetPaymentInfoByPaymentRequest(PaymentInfoInputDto paymentInfoInput);
        Task<PaymentRequestFullOutputDto> CreateRequest(PaymentRequestInsertDto paymentRequestInsert);
        Task<bool> CancelRequest(PaymentCancelInputDto PaymentRequestDto);
        Task<CreatePaymentTransactionOutputDto> CreatePaymentRequest(CreatePaymentTransactionInputDto inputDto);

        Task<bool> CheckExists(Guid id);
        Task<bool> CheckExistsByCode(string paymentRequestCode);
        Task<PaymentRequestFullOutputDto> GetById(Guid id);
        Task<PaymentRequestFullOutputDto> GetByPaymentRequestCode(string paymentRequestCode);
        Task<bool> CheckPaymentRequestCodeExists(string paymentRequestCode);
        Task<List<PaymentRequestFullOutputDto>> GetListByOrderCode(string orderCode);
        //Task<UploadFileOutputDto> UploadFile(IFormFile file);
        Task<PaymentDepositInfoOutputDto> GetPaymentDepositInfoByPaymentRequest(PaymentDepositInfoInputDto inputDto);
        Task<CreateWithdrawDepositTransferOutputDto> CreateWithdrawDepositTransfer(CreateWithdrawDepositTransferInputDto inputDto);
        Task<CreatePaymentTransactionOutputDto> CreateWithdrawDepositCash(CreateWithdrawDepositInputDto inputDto);
        Task<bool> UpdateCompanyCod(UpdateCompanyInfoInputDto inItem);
        Task<GetPresignUploadOutputDto> GetPresignUploadS3(GetPresignUploadInputDto input);
        Task<TransferFullOutputDto> GetTranferByPaymentRequestCode(string paymentRequestCode);
        Task<TransferFullOutputDto> GetTranferByPaymentRequest(SearchTransferCondi item);
        Task<PaymentDepositRequestIdInfoOutputDto> GetPaymentDepositInfoByPaymentRequestId(PaymentDepositInfoInputDto inputDto);
        Task<TransferFullOutputDto> Testnuget(string paymentRequestCode);
        Task<PaymentAccountingOutputDto> GetPaymentInfoByPaymentRequestId(PaymentInfoInputDto paymentInfoInput);
        Task<TransferFullOutputDto> GetTranferByPaymentCode(string paymentCode);
        Task<List<PaymentInfoDetailDto>> MapDetailTransactionRecharge(List<TransactionFullOutputDto> transRecharge);
        Task<PaymentRequestFullOutputDto> GetByPaymentCode(string paymentCode);
        Task<PaymentRequestFullOutputDto> GetByPayment(string paymentRequestCode);
    }
    public interface IDepositService : IPaymentCoreAppServiceBase
    {
        Task<DepositByCashOutputDto> DepositByCash(DepositByCashInputDto inItem);
        Task<DepositByEWalletOutputDto> DepositByEWallet(DepositByEWalletInputDto inItem);
        Task<DepositByCardOutputDto> DepositByCard(DepositByCardInputDto inItem);
        Task<DepositByCODOutputDto> DepositByCOD(DepositByCODInputDto inItem);
        Task<DepositByTransferOutputDto> DepositByTransfer(DepositByTransferInputDto inItem);
        Task<DepositByVoucherOutputDto> DepositByVoucher(DepositByVoucherInputDto inItem);
        Task<DepositByEWalletOutputDto> DepositByEWalletOnline(DepositByEWalletOnlineInputDto inItem);
        Task<DepositByMultipleVoucherOutputDto> DepositByMultipleVoucher(DepositByVoucherInputDto inItem);
        Task<DepositDebtSaleOutputDto> DepositDebtSale(DepositDebtSaleInputDto inItem);
    }
}
