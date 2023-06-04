using FRTTMO.PaymentCore.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace FRTTMO.PaymentCore.Permissions
{
    public class PaymentCorePermissionDefinitionProvider : PermissionDefinitionProvider
    {
        public override void Define(IPermissionDefinitionContext context)
        {
            var myGroup = context.AddGroup(PaymentCorePermissions.GroupName, L("Permission:PaymentCore"));
        }

        private static LocalizableString L(string name)
        {
            return LocalizableString.Create<PaymentCoreResource>(name);
        }
    }
}