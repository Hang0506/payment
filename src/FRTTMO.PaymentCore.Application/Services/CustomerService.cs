using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Entities;
using FRTTMO.PaymentCore.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace FRTTMO.PaymentCore.Services
{
    public class CustomerService : PaymentCoreAppService, ICustomerService, ITransientDependency
    {
        private readonly ILogger<CustomerService> _log;
        private readonly ICustomerRepository _customerRepository;
        private readonly IAccountRepository _accountRepository;
        IRepository<Customer, Guid> _repository;
        public CustomerService(
                            ILogger<CustomerService> log,
                            ICustomerRepository customerRepository,
                            IAccountRepository accountRepository,
                            IRepository<Customer, Guid> repository
                            )
        {
            _log = log;
            _customerRepository = customerRepository;
            _accountRepository = accountRepository;
            _repository = repository;
        }
        /// <summary>
        /// API tạo mới customer
        /// </summary>
        /// <param name="inputDto"></param>
        /// <returns></returns>
        public async Task<CustomerFullOutputDto> CreateAsync(CustomerInsertInputDto inputDto)
        {
            try
            {
                //valid 
                ValidationInput(inputDto);

                // Kiểm tra tồn tại  Customer
                var existsCustomer = await CheckExistsCustomerAsync(inputDto);
                if (existsCustomer) throw new BusinessException(PaymentCoreErrorCodes.ERROR_CUSTOMER_EXISTS).WithData("Result", false);

                //insert customer
                var customer = ObjectMapper.Map<CustomerInsertInputDto, Customer>(inputDto);
                customer = await _customerRepository.CreateAsync(customer);

                // insert account
                if (customer != null)
                {
                    Account account = new Account()
                    {
                        CustomerId = customer.Id,
                        AccountNumber = Guid.NewGuid(),
                        CurrentBalance = 0,
                        CreatedDate = DateTime.Now,
                        CreatedBy = customer.CreatedBy,
                    };
                    await _accountRepository.CreateAsync(account);
                }
                //trả về kết quả
                return ObjectMapper.Map<Customer, CustomerFullOutputDto>(customer);
            }
            catch (BusinessException bex)
            {
                throw bex;
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", ex.Message);
            }

        }
        /// <summary>
        /// API check thông tin customer 
        /// </summary>
        /// <param name="requestDto"></param>
        /// <returns></returns>
        public async Task<CustomerFullOutputDto> VerifyCustomerAsync(VerifyCustomerRequestDto requestDto)
        {
            try
            {
                //_log.LogInformation("VerifyCustomerAsync Request body: {0}", JsonConvert.SerializeObject(requestDto));
                // kiểm tra ít nhất phải có 1 trường có giá trị
                if (!(requestDto.Id.HasValue || !string.IsNullOrEmpty(requestDto.TaxNumber) || !string.IsNullOrEmpty(requestDto.Mobile)))
                {
                    throw new BusinessException(PaymentCoreErrorCodes.ERROR_CUSTOMER_NOTFOUND).WithData("Result", false);
                }
                // where
                Expression<Func<Customer, bool>> expression = (customer) =>
                    (requestDto.Id == null || customer.Id == requestDto.Id)
                    && (string.IsNullOrEmpty(requestDto.Mobile) || customer.Mobile == requestDto.Mobile)
                    && (string.IsNullOrEmpty(requestDto.TaxNumber) || customer.TaxNumber == requestDto.TaxNumber);

                var customer = await _customerRepository.VerifyCustomerAsync(expression);

                if (customer != null)
                {
                    // Nếu tồn tại  và còn hiệu lực (Status = 1) thì trả kết quả 
                    if (customer.Status == Common.EnumType.CustomerStatus.Active)
                    {
                        return ObjectMapper.Map<Customer, CustomerFullOutputDto>(customer);
                    }
                    // Nếu tồn tại  và hết hiệu lực (Status = 0) thì trả kết quả Error code và Message lỗi 00702 - ERROR_CUSTOMER_INACTIVE
                    else
                    {
                        throw new BusinessException(PaymentCoreErrorCodes.ERROR_CUSTOMER_INACTIVE).WithData("Result", false);
                    }

                }
                //Nếu không tồn tại thì trả về Error code và Message lỗi: 00701 - ERROR_CUSTOMER_NOTFOUND
                else
                {
                    throw new BusinessException(PaymentCoreErrorCodes.ERROR_CUSTOMER_NOTFOUND).WithData("Result", false);
                }
            }
            catch (BusinessException bex)
            {
                throw bex;
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", ex.Message);
            }

        }
        /// <summary>
        /// kiểm tra tồn tai
        /// </summary>
        /// <param name="requestDto"></param>
        /// <returns></returns>
        private async Task<bool> CheckExistsCustomerAsync(CustomerInsertInputDto input)
        {
            if (input == null) return true;
            Customer customer = null;
            if (input.CustomerType == Common.EnumType.CustomerType.Company)
            {
                customer = await _repository.FirstOrDefaultAsync(w => (w.FullName == input.FullName || w.TaxNumber == input.TaxNumber) && w.CustomerType == Common.EnumType.CustomerType.Company);
            }
            else
            {
                customer = await _repository.FirstOrDefaultAsync(w => w.Mobile == input.Mobile && w.CustomerType == Common.EnumType.CustomerType.Individual);
            }
            return customer != null;
        }
        private void ValidationInput(CustomerInsertInputDto inputDto)
        {
            /*if (!PaymentCoreUtilities.IsValidEmail(inputDto.Email))
            {
                throw new BusinessException(PaymentCoreErrorCodes.ERROR_CUSTOMER_EMAIL).WithData(nameof(inputDto.Email), false);
            }*/
            if (inputDto.FullName == null || string.IsNullOrEmpty(inputDto.FullName)) throw new BusinessException(PaymentCoreErrorCodes.ERROR_CUSTOMER_FULL_NAME).WithData(nameof(inputDto.FullName), false);
            if (inputDto.CustomerType == Common.EnumType.CustomerType.Company)
            {
                if (inputDto.TaxNumber == null || string.IsNullOrEmpty(inputDto.TaxNumber)) throw new BusinessException(PaymentCoreErrorCodes.ERROR_CUSTOMER_TAX_NUMBER).WithData(nameof(inputDto.TaxNumber), false);
                if (inputDto.Address == null || string.IsNullOrEmpty(inputDto.Address)) throw new BusinessException(PaymentCoreErrorCodes.ERROR_CUSTOMER_ADDRESS).WithData(nameof(inputDto.Address), false);
            }
            if (inputDto.CustomerType == Common.EnumType.CustomerType.Individual)
            {
                if (inputDto.Mobile == null || string.IsNullOrEmpty(inputDto.Mobile)) throw new BusinessException(PaymentCoreErrorCodes.ERROR_CUSTOMER_MOBILE).WithData(nameof(inputDto.Mobile), false);
            }
        }
    }
}
