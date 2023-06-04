using FRTTMO.PaymentCore.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FRTTMO.PaymentCore.Services
{
    public interface ICODService
    { 
        Task<CODFullOutputDto> Insert(CODInputDto dtoInput);
        Task<List<CODFullOutputDto>> GetByTransactionIds(List<Guid> transIds);
        Task<List<CODFullOutputDto>> GetByTransactionId(Guid transId);
    }
}
