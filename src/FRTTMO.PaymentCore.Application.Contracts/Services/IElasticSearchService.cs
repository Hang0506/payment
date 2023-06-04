using FRTTMO.PaymentCore.Application.Contracts;
using FRTTMO.PaymentCore.Dto.TransactionDetailDto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Services
{
    public interface IElasticSearchService : IPaymentCoreAppServiceBase
    {
        Task<bool> SyncDataESTransfer(string paymentCode, TransactionDetailTransferOutputDto data = null);
        Task<List<TransactionDetailTransferOutputDto>> SearchESByPaymentCode(SearchESByPaymentCodeRequestDto request);
        Task<TransactionDetailTransferOutputDto> GetDocTransferES(string paymentCode);
        Task<bool> DeleteDataESTransfer(string paymentCode);
        Task<TransferInfoDetailOutputDto> SearchESTransferInfoByPaymentCode(TransferInfoInputDto request);
    }
}
