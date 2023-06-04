using System;
using System.Collections.Generic;
using System.Text;

namespace FRTTMO.PaymentCore.Application.Redis.Redis
{
    using StackExchange.Redis;
    public interface IRedisConnectionFactory
    {
        ConnectionMultiplexer Connection();
        string ConnectionString();
    }
}
