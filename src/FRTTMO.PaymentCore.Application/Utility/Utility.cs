using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace FRTTMO.PaymentCore
{
    public class EnvironmentSetting
    {
        public const string RemoteOMSService = "RemoteOMSService";
        public const string PaymentIntegration = "PaymentIntegration";
        public const string RemoteDebitService = "DebitService";
        public const string AWSS3 = "AWSS3";
    }
    public class Decription
    {
        public const string DepositError = "Transaction đã tồn tại !";
        public const string PaymentTransaction = "Tự động Gạch Tiền";
    }
    public static class Utility
    {
        /// <summary>
        /// Encode string to SHA256
        /// </summary>
        public static string SHA256Hash(string input)
        {
            using (SHA256 mySHA256 = SHA256.Create())
            {
                var plainBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = mySHA256.ComputeHash(plainBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }
       
        public static string Base64Encode(string plainText) => Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
        public static int UnixTimestamp(DateTime? InDate = null) => (int)(InDate ?? DateTime.UtcNow).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

        /// <summary>
        /// Encode string to MD5
        /// </summary>
        public static string MD5Hash(string input)
        {
            var hash = new StringBuilder();
            var md5Provider = new MD5CryptoServiceProvider();
            byte[] bytes = md5Provider.ComputeHash(Encoding.UTF8.GetBytes(input));
            for (int i = 0; i < bytes.Length; i++)
            {
                hash.Append(bytes[i].ToString("X2"));
            }
            return hash.ToString();
        }

        public static T LowercaseModelToModel<T>(object result)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (PropertyInfo pi in result.GetType().GetProperties())
            {
                if (typeof(String).IsAssignableFrom(pi.PropertyType) || (!typeof(IList).IsAssignableFrom(pi.PropertyType) && !pi.PropertyType.IsClass))
                {
                    foreach (PropertyInfo pro in temp.GetProperties())
                    {
                        if (pi.Name.ToLower() == pro.Name.ToLower())
                        {
                            pro.SetValue(obj, pi.GetValue(result), null);
                            break;
                        }
                    }
                }
            }

            return obj;
        }

        public static List<T> LowercaseListModelToListModel<T>(IList result)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();
            var listObj = new List<T>();

            if (result == null)
            {
                return listObj;
            }

            foreach (var item in result)
            {
                foreach (PropertyInfo pi in item.GetType().GetProperties())
                {
                    if (typeof(String).IsAssignableFrom(pi.PropertyType) || (!typeof(IList).IsAssignableFrom(pi.PropertyType) && !pi.PropertyType.IsClass))
                    {
                        foreach (PropertyInfo pro in temp.GetProperties())
                        {
                            if (pi.Name.ToLower() == pro.Name.ToLower())
                            {
                                pro.SetValue(obj, pi.GetValue(item), null);
                                break;
                            }
                        }
                    }
                }
                listObj.Add(obj);
            }

            return listObj;
        }
    }

}
