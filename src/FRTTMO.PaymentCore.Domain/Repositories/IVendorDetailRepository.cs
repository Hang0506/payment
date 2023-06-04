using FRTTMO.PaymentCore.Entities;
using System.Threading.Tasks;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Repositories
{
    public interface IVendorDetailRepository
    {
        Task<VendorDetail> GetByPartnerIdIdAsync(EnmPartnerId? partnerId);
    }
}
