using System;

namespace FRTTMO.PaymentCore
{
    public class PaymentCoreUtilities
    {
        public static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        public static DateTime GetDate(string code)
        {
            try
            {
                var paymentArr = code.Split("-")[1];
                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(paymentArr));
                return dateTimeOffset.LocalDateTime.Date;
            }
            catch
            {
                return new(2022, 6, 19, 0, 0, 0, 0, DateTimeKind.Utc);
            }
        }
    }
}
