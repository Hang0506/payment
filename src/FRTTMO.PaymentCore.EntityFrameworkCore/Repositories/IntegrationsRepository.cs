using Volo.Abp.DependencyInjection;

namespace FRTTMO.PaymentCore.Repositories
{
    public class IntegrationsRepository : IIntegrationsRepository, ITransientDependency
    {
        //private readonly IRepository<xx, xx> _repository;

        public IntegrationsRepository(/*IRepository<xx, xx> _repository*/) : base()
        {
            //_repository = repository;
        }
       
    }
}
