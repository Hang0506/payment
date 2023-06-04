using FRTTMO.PaymentCore.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using static FRTTMO.PaymentCore.Common.EnumType;

namespace FRTTMO.PaymentCore.Repositories
{
    public class CancelDepositRepository : ITransientDependency, ICancelDepositRepository
    {
        private readonly IRepository<PaymentMethod, int> _repositoryPaymentMethod;
        private readonly IRepository<PaymentMethodDetail, int> _repositoryPaymentMethodDetail;

        public CancelDepositRepository(
            IRepository<PaymentMethod, int> repositoryPaymentMethod,
            IRepository<PaymentMethodDetail, int> repositoryPaymentMethodDetail)
        {
            _repositoryPaymentMethod = repositoryPaymentMethod;
            _repositoryPaymentMethodDetail = repositoryPaymentMethodDetail;
        }

        public async Task<List<PaymentMethod>> GetlistpaymentpaymethodAsync()
        {
            var respponse = new List<PaymentMethod>();
            var PaymentMethod = await _repositoryPaymentMethod.GetListAsync(method => method.Type.Equals(EnumPaymentMethodType.Common) || method.Type.Equals(EnumPaymentMethodType.DepositRefund));
            foreach (var item in PaymentMethod)
            {
                var respponseItem = new PaymentMethod();
                respponseItem = item;
                respponseItem.Detail = await _repositoryPaymentMethodDetail.GetListAsync(method => method.PaymentMethodId.Equals(item.Id));
                respponse.Add(respponseItem);
            }
            return respponse;
        }
    }
}
