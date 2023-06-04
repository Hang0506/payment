using FluentValidation;
using FRTTMO.PaymentCore.Dto;

namespace FRTTMO.PaymentCore.Validations.FluentValidations
{
    public class RefundDtoValidator : AbstractValidator<RefundDto>
    {
        public RefundDtoValidator()
        {
            RuleFor(x => x.OrderCode).NotNull().Length(1, 40);
            RuleFor(x => x.ShopCode).MaximumLength(10);
            RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Note).MaximumLength(200);
        }
    }
}
