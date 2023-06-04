using FRTTMO.PaymentCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Repositories
{
    public interface IBankingOnlineRepository
    {
        Task<List<BankingOnline>> GetAllAsync();
    }
}
