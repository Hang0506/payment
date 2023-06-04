using FluentValidation;
using FRTTMO.PaymentCore.Dto;

namespace FRTTMO.PaymentCore.Validations.FluentValidations
{
    /*public class PaymentInfoInputDtoValidator : AbstractValidator<PaymentInfoInputDto>
    {
        public PaymentInfoInputDtoValidator()
        {
            RuleFor(x => x.PaymentRequestId).NotNull();
        }
    }*/
    public class PaymentRequestInsertDtoValidator : AbstractValidator<PaymentRequestInsertDto>
    {
        public PaymentRequestInsertDtoValidator()
        {
            RuleFor(x => x.OrderCode).NotNull().Length(1, 40);
            RuleFor(x => x.TotalPayment).GreaterThanOrEqualTo(0);
            RuleFor(x => x.CreatedBy).MaximumLength(50);
        }
    }
    public class PaymentCancelInputDtoValidator : AbstractValidator<PaymentCancelInputDto>
    {
        public PaymentCancelInputDtoValidator()
        {
            RuleFor(x => x.PaymentRequestCode).NotNull();
        }
    }
    public class CreatePaymentTransactionInputDtoValidator : AbstractValidator<CreatePaymentTransactionInputDto>
    {
        public CreatePaymentTransactionInputDtoValidator()
        {
            RuleFor(x => x.PaymentRequestCode).NotNull().MaximumLength(20);
            RuleFor(x => x.OrderCode).NotNull().Length(1, 40);
            RuleFor(x => x.Transaction).SetValidator(new PaymentTransactionInputDtoValidator());
        }
    }
    public class PaymentTransactionInputDtoValidator : AbstractValidator<PaymentTransactionInputDto>
    {
        public PaymentTransactionInputDtoValidator()
        {
            RuleFor(x => x.ShopCode).MaximumLength(10);
            RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Note).MaximumLength(200);
            RuleFor(x => x.CreatedBy).MaximumLength(50);
        }
    }
    public class CreateWithdrawDepositInputDtoValidator : AbstractValidator<CreateWithdrawDepositInputDto>
    {
        public CreateWithdrawDepositInputDtoValidator()
        {
            //RuleFor(x => x.PaymentCode).NotNull();
            RuleFor(x => x.Transaction).SetValidator(new PaymentTransactionInputDtoValidator());
        }
    }
    public class CreateWithdrawDepositTransferInputDtoValidator : AbstractValidator<CreateWithdrawDepositTransferInputDto>
    {
        public CreateWithdrawDepositTransferInputDtoValidator()
        {
            //RuleFor(x => x.PaymentCode).NotNull();
            RuleFor(x => x.Transaction).SetValidator(new PaymentTransactionInputDtoValidator());
            RuleForEach(x => x.Transfers).SetValidator(new PaymentTransferInputDtoValidator());
        }
    }
}
