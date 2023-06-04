using FluentValidation;
using FRTTMO.PaymentCore.Dto;

namespace FRTTMO.PaymentCore.Validations.FluentValidations
{
    public class CashbackInputBaseDtoValidator : AbstractValidator<CashbackInputBaseDto>
    {
        public CashbackInputBaseDtoValidator()
        {
            RuleFor(x => x.OrderCode).NotNull().Length(1, 40);
            RuleFor(x => x.Totalpayment).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Transaction).SetValidator(new TransactionInputDtoValidator());
        }
    }
}
