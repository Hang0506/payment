using FluentValidation;
using FRTTMO.PaymentCore.Dto;

namespace FRTTMO.PaymentCore.Validations.FluentValidations
{
    public class ReturnCashDtoValidator : AbstractValidator<ReturnCashDto>
    {
        public ReturnCashDtoValidator()
        {
            RuleFor(x => x.Totalpayment).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Transaction).SetValidator(new TransactionReturnDtoValidator());
        }
    }
    public class TransactionReturnDtoValidator : AbstractValidator<TransactionReturnDto>
    {
        public TransactionReturnDtoValidator()
        {
            RuleFor(x => x.ShopCode).MaximumLength(10);
            RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Note).MaximumLength(200);
        }
    }
    public class ReturnTransferDtoValidator : AbstractValidator<ReturnTransferDto>
    {
        public ReturnTransferDtoValidator()
        {
            RuleFor(x => x.ShopCode).MaximumLength(10);
            RuleFor(x => x.Transfers).SetValidator(new PaymentTransferInputDtoValidator());
        }
    }
}
