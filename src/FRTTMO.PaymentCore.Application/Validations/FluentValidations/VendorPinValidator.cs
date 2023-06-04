using FluentValidation;
using FRTTMO.PaymentCore.Dto;

namespace FRTTMO.PaymentCore.Validations.FluentValidations
{
    public class InsertVenderInputDtoValidator : AbstractValidator<InsertVenderInputDto>
    {
        public InsertVenderInputDtoValidator()
        {
            RuleFor(x => x.ShopCode).MaximumLength(20);
            RuleFor(x => x.PinCode).MaximumLength(20);
        }
    }
    public class UpdateVenderInputDtoValidator : AbstractValidator<UpdateVenderInputDto>
    {
        public UpdateVenderInputDtoValidator()
        {
            RuleFor(x => x.PinCode).MaximumLength(20);
        }
    }
}
