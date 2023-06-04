using FluentValidation;
using FRTTMO.PaymentCore.Dto;

namespace FRTTMO.PaymentCore.Validations.FluentValidations
{
    public class DepositByVoucherInputDtoValidator : AbstractValidator<DepositByVoucherInputDto>
    {
        public DepositByVoucherInputDtoValidator()
        {
            RuleFor(x => x).SetValidator(new DepositInputBaseDtoValidator());
        }
    }
    public class DepositInputBaseDtoValidator : AbstractValidator<DepositInputBaseDto>
    {
        public DepositInputBaseDtoValidator()
        {
            RuleFor(x => x.OrderCode).NotNull();//.NotEmpty().WithMessage(PaymentCoreErrorCodes.ERROR_ORDER_CODE_NOT_EMPTY);
            RuleFor(x => x.Transaction).SetValidator(new TransactionInputDtoValidator());
        }
    }
    public class TransactionInputDtoValidator : AbstractValidator<InsertTransactionInputDto>
    {
        public TransactionInputDtoValidator()
        {
            RuleFor(x => x.AccountId).NotNull();//.NotEqual(Guid.Empty);
            RuleFor(e => e.TransactionTypeId).NotNull();
            RuleFor(e => e.PaymentRequestId).NotNull();
            RuleFor(e => e.ShopCode).NotNull().Length(1, 10);
            //RuleFor(e => e.PaymentMethodId).NotNull();
            RuleFor(e => e.Amount).NotNull();//.GreaterThan(0m);
            RuleFor(e => e.Note).MaximumLength(200);
            RuleFor(e => e.CreatedBy).MaximumLength(50);
        }
    }
}
