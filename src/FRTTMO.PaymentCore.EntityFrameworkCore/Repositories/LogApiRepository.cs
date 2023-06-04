using FRTTMO.PaymentCore.Entities;
using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace FRTTMO.PaymentCore.Repositories
{
    public class LogApiRepository : ILogApiRepository, ITransientDependency
    {
        private readonly IRepository<LogApi, int> _repository;

        public LogApiRepository(IRepository<LogApi, int> repository)
        {
            _repository = repository;
        }

        public async Task<LogApi> InsertAsync(LogApi logApi)
        {
            logApi.CreatedDate = DateTime.Now;
            logApi = await _repository.InsertAsync(logApi, true);
            return logApi;
        }
    }
}
