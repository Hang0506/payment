using Volo.Abp.Reflection;

namespace FRTTMO.PaymentCore.Permissions
{
    public class PaymentCorePermissions
    {
        public const string GroupName = "PaymentCore";

        public static string[] GetAll()
        {
            return ReflectionHelper.GetPublicConstantsRecursively(typeof(PaymentCorePermissions));
        }
    }
}