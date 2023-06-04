using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Entities;
using FRTTMO.PaymentCore.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace FRTTMO.PaymentCore.Services
{
    public class VendorPinService : PaymentCoreAppService, ITransientDependency, IVendorPinService
    {
        private readonly IVendorPinRepository _repository;

        public VendorPinService(
            IVendorPinRepository repository
        ) : base()
        {
            _repository = repository;
        }

        public async Task<VendorPinDto> GetByVendor(int vendorId, string shopCode)
        {
            var rt = await _repository.GetByVendor(vendorId, shopCode);
            if (rt == null) throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION).WithData("Data", "Vendor/Shop");
            return ObjectMapper.Map<VendorPin, VendorPinDto>(rt);
        }

        public async Task<List<VendorPinFullOutputDto>> GetListAsync()
        {
            var rt = await _repository.GetListAsync();
            if (rt == null) throw new BusinessException(PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION).WithData("Data", "Vendor/Shop");
            return ObjectMapper.Map<List<VendorPin>, List<VendorPinFullOutputDto>>(rt);
        }

        public async Task<VendorPinFullOutputDto> InsertAsync(InsertVenderInputDto input)
        {
            var item = await _repository.GetByVendor(input.VendorId.Value, input.ShopCode);
            if (item != null) throw new BusinessException(PaymentCoreErrorCodes.ERROR_DATA_EXISTED_EXCEPTION).WithData("Data", "Vendor");
            var vendorEntity = ObjectMapper.Map<InsertVenderInputDto, VendorPin>(input);
            vendorEntity.CreatedDate = DateTime.Now;
            var rt = await _repository.InsertAsync(vendorEntity);
            if (rt == null) throw new BusinessException(PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION).WithData("Data", "Vendor/Shop");
            return ObjectMapper.Map<VendorPin, VendorPinFullOutputDto>(rt);
        }

        public async Task<VendorPinFullOutputDto> UpdateAsync(int id, UpdateVenderInputDto input)
        {
            var entity = await _repository.GetById(id);
            if (entity == null) throw new BusinessException(PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION).WithData("Data", "Vendor/Shop");
            ObjectMapper.Map(input, entity);
            entity.ModifiedDate = DateTime.Now;
            var rt = await _repository.UpdateAsync(entity);
            return ObjectMapper.Map<VendorPin, VendorPinFullOutputDto>(rt);
        }

        public async Task<VendorPinFullOutputDto> DeleteAsync(int id)
        {
            var entity = await _repository.GetById(id);
            if (entity == null) throw new BusinessException(PaymentCoreErrorCodes.ERROR_DATA_NULL_EXCEPTION).WithData("Data", "Vendor/Shop");
            var rt = await _repository.DeleteAsync(entity);
            return ObjectMapper.Map<VendorPin, VendorPinFullOutputDto>(rt);
        }
    }
}
