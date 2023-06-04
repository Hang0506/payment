using FluentValidation;
using FRTTMO.PaymentCore.Dto;

namespace FRTTMO.PaymentCore.Validations.FluentValidations
{
    public class DepositByEWalletInputDtoValidator : AbstractValidator<DepositByEWalletInputDto>
    {
        public DepositByEWalletInputDtoValidator()
        {
            RuleFor(x => x).SetValidator(new DepositInputBaseDtoValidator());
            RuleFor(x => x.EWallet).NotNull();
            RuleFor(x => x.EWallet).SetValidator(new PaymentEWalletDepositInputDtoValidator());
        }
    }
    public class PaymentEWalletDepositInputDtoValidator : AbstractValidator<PaymentEWalletDepositInputDto>
    {
        public PaymentEWalletDepositInputDtoValidator()
        {
            RuleFor(x => x.TransactionVendor).MaximumLength(50);
            RuleFor(x => x.CreatedBy).MaximumLength(50);
            RuleFor(x => x.TypeWalletId).NotNull();
            RuleFor(e => e.Amount).NotNull().GreaterThanOrEqualTo(0m); 
        }
    }
}
