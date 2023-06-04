using FRTTMO.PaymentCore.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Repositories
{
    public class VendorDetailRepository : ITransientDependency, IVendorDetailRepository
    {
        private readonly IRepository<VendorDetail, int> _repository;

        public VendorDetailRepository(IRepository<VendorDetail, int> repository)
        {
            _repository = repository;
        }

        public async Task<VendorDetail> GetByPartnerIdIdAsync(EnmPartnerId? PartnerId) => await _repository.FirstOrDefaultAsync(c => c.PartnerId == PartnerId && c.Active == true);
    }
}