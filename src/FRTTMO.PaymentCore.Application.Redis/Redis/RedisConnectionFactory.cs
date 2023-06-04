using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using Volo.Abp.DependencyInjection;

namespace FRTTMO.PaymentCore.Application.Redis.Redis
{
    public class RedisConnectionFactory : IRedisConnectionFactory, ISingletonDependency
    {
        private readonly FdxRedisOptions _option;
        /// <summary>
        ///     The _connection.
        /// </summary>
        private readonly Lazy<ConnectionMultiplexer> _connection;

        public RedisConnectionFactory(IOptions<FdxRedisOptions> option)
        {
            _option = option.Value;
            this._connection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(_option.RedisConnectionString));
        }

        public ConnectionMultiplexer Connection()
        {
            return this._connection.Value;
        }

        public string ConnectionString()
        {
            return this._option.RedisConnectionString.Split(',')[0];
        }
    }
}
