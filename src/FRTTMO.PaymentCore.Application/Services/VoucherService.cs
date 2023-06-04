using FRTTMO.PaymentCore.Dto;
using FRTTMO.PaymentCore.Entities;
using FRTTMO.PaymentCore.Repositories;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace FRTTMO.PaymentCore.Services
{
    public class VoucherService : PaymentCoreAppService, ITransientDependency, IVoucherService
    {
        private readonly ILogger<VoucherService> _log;
        private readonly IVoucherRepository _voucherRepository;

        public VoucherService(IVoucherRepository voucherRepository, ILogger<VoucherService> log)
        {
            _voucherRepository = voucherRepository;
            _log = log;
        }

        public async Task<List<VoucherFullOutputDto>> GetByTransactionId(Guid transId)
        {
            var vouchers = await _voucherRepository.GetByTransactionId(transId);
            var voucherDtos = vouchers.Select(ObjectMapper.Map<Voucher, VoucherFullOutputDto>).ToList();
            return voucherDtos;
        }
        public async Task<VoucherFullOutputDto> InsertVoucher(VoucherInputDto voucher)
        {
            try
            {
                var voucherEntityInput = ObjectMapper.Map<VoucherInputDto, Voucher>(voucher);
                var voucherEntityResult = await _voucherRepository.InsertVoucher(voucherEntityInput);
                return ObjectMapper.Map<Voucher, VoucherFullOutputDto>(voucherEntityResult);
            }
            catch (BusinessException e)
            {
                _log.LogError(string.Format("InsertVoucher PaymentCore: {0}| Request body: {1} ", e, JsonConvert.SerializeObject(voucher)));
                throw e;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, _CoreName + "InsertVoucher-Request body: {RequestObject}", voucher);
                throw new UserFriendlyException(null, PaymentCoreErrorCodes.ERROR_EXCEPTION).WithData("Message", ex.Message);
            }
        }

        public async Task<List<VoucherFullOutputDto>> GetByTransactionIds(List<Guid> transIds)
        {
            var vouchers = await _voucherRepository.GetByTransactionIds(transIds);
            var voucherDtos = vouchers.Select(ObjectMapper.Map<Voucher, VoucherFullOutputDto>).ToList();
            return voucherDtos;
        }
    }
}
