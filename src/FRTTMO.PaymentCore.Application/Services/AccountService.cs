using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Entities;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace FRTTMO.PaymentCore.Services
{
    public class AccountService : PaymentCoreAppService, IAccountService
    {
        readonly IRepository<Account, Guid> _repository;

        public AccountService(IRepository<Account, Guid> repository) : base()
        {
            _repository = repository;
        }
        public async Task<AccountFullOutputDto> CreateAsync(AccountInsertInputDto inputDto)
        {
            var account = ObjectMapper.Map<AccountInsertInputDto, Account>(inputDto);
            var checkExists = await _repository.FirstOrDefaultAsync(w => w.CustomerId == inputDto.CustomerId);
            if (checkExists != null) throw new BusinessException(PaymentCoreErrorCodes.ERROR_CUSTOMER_EXISTS).WithData("Result", false);
            var accountOutput = await _repository.InsertAsync(account, true);
            return ObjectMapper.Map<Account, AccountFullOutputDto>(accountOutput);
        }
        public async Task<AccountFullOutputDto> GetBalanceByCustomerId(Guid customerId)
        {
            if (customerId == Guid.Empty) throw new BusinessException(PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION).WithData("Data", customerId);
            var account = await _repository.FirstOrDefaultAsync(w => w.CustomerId == customerId);
            if (account == null) throw new BusinessException(PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION).WithData("Data", customerId);
            return ObjectMapper.Map<Account, AccountFullOutputDto>(account);
        }
    }
}
