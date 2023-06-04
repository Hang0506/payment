using FRTTMO.PaymentCore.Dto;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Json;

namespace FRTTMO.PaymentCore.Application.Redis
{
    public abstract class RedisBaseService<T> where T : RedisItemDto
    {
        protected IJsonSerializer _jsonSerializer { get; set; }

        /// <summary>
        /// Generates a key for a Redis Entry  , follows the Redis Name Convention of inserting a colon : to identify values
        /// </summary>
        /// <param name="key">Redis identifier key</param>
        /// <returns>concatenates the key with the name of the type</returns>
        protected string GenerateKey(string key)
        {
            //return string.Concat(key.ToLower(), ":", this.Name.ToLower());
            return key;
        }

        public byte[] Serialize<E>(E obj)
        {
            return Encoding.UTF8.GetBytes(_jsonSerializer.Serialize(obj));
        }

        public T Deserialize(byte[] bytes)
        {
            return (T)_jsonSerializer.Deserialize(typeof(T), Encoding.UTF8.GetString(bytes));
        }
        public IList<T> Deserialize(RedisValue[] values)
        {
            List<T> list = new List<T>();
            foreach (var v in values)
            {
                //remove empty value from redis
                if (v.HasValue)
                {
                    list.Add((T)_jsonSerializer.Deserialize(typeof(T), Encoding.UTF8.GetString(v)));
                }
            }
            return list;
        }

        public IDictionary<string, T> DeserializeToDictionary(HashEntry[] values)
        {
            IDictionary<string, T> dic = new Dictionary<string, T>();
            foreach (HashEntry v in values)
            {

                try
                {
                    T value = (T)_jsonSerializer.Deserialize(typeof(T), Encoding.UTF8.GetString(v.Value));
                    dic.Add(v.Name, value);


                }
                catch (JsonSerializationException ex)
                {
                    //ignore item:imei code
                    //Console.WriteLine($"error with serialize data: {Encoding.UTF8.GetString(v.Value)}");
                }
                catch (JsonReaderException exRead)
                {
                    //ignore item:imei code
                    //Console.WriteLine($"error with reader data: {Encoding.UTF8.GetString(v.Value)}");
                }
            }
            return dic;
        }

        public IDictionary<string, T> DeserializeToDictionary(RedisValue[] values)
        {
            IDictionary<string, T> dic = new Dictionary<string, T>();
            List<T> list = new List<T>();
            foreach (var v in values)
            {
                try
                {
                    //remove empty value from redis
                    if (v.HasValue)
                    {
                        T value = (T)_jsonSerializer.Deserialize(typeof(T), Encoding.UTF8.GetString(v));
                        dic.Add(value.PaymentCode, value);
                    }
                }
                catch (JsonSerializationException ex)
                {
                    Console.WriteLine($"error with serialize data: {Encoding.UTF8.GetString(v)}");
                }
                catch (JsonReaderException exRead)
                {
                    Console.WriteLine($"error with reader data: {Encoding.UTF8.GetString(v)}");
                }
            }
            return dic;
        }
    }
}
