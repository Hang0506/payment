using FluentValidation;
using FRTTMO.PaymentCore.Dto;

namespace FRTTMO.PaymentCore.Validations.FluentValidations
{
    public class CancelDepositDtoValidator : AbstractValidator<CancelDepositDto>
    {
        public CancelDepositDtoValidator()
        {
            RuleFor(x => x.OrderCode).MaximumLength(40).NotNull();
            RuleFor(x => x.CreatedBy).MaximumLength(50);
            RuleFor(x => x.TotalPayment).GreaterThanOrEqualTo(0m); // check >= 0 k
            RuleFor(x => x.Transaction).SetValidator(new InsertTransactionCancelInputDtoValidator());
        }
    }
    public class InsertTransactionCancelInputDtoValidator : AbstractValidator<InsertTransactionCancelInputDto>
    {
        public InsertTransactionCancelInputDtoValidator()
        {
            RuleFor(x => x.AccountId).NotNull();//.NotEqual(Guid.Empty);
            RuleFor(e => e.TransactionTypeId).NotNull();
            RuleFor(e => e.ShopCode).NotNull().Length(1, 10);
            RuleFor(e => e.Amount).NotNull();//.GreaterThan(0m);
            RuleFor(e => e.Note).MaximumLength(200);
            RuleFor(e => e.CreatedBy).MaximumLength(50);
        }
    }
    public class PaymentRequestTransferDtoValidator : AbstractValidator<PaymentRequestTransferDto>
    {
        public PaymentRequestTransferDtoValidator()
        {
            RuleFor(x => x.OrderCode).Length(1, 40).NotNull();
            RuleFor(x => x.CreatedBy).MaximumLength(50);
        }
    }
    public class TransactionCancelDepositTransferValidator : AbstractValidator<TransactionCancelDepositTransfer>
    {
        public TransactionCancelDepositTransferValidator()
        {
            RuleFor(x => x.PaymentRequestId).NotNull().NotEmpty();
            RuleFor(x => x.CustomerId).NotNull().NotEmpty();
            RuleFor(e => e.ShopCode).NotNull().Length(1, 10);
            RuleFor(x => x.Transfers).SetValidator(new PaymentTransferInputDtoValidator());
        }
    }
    public class PaymentTransferInputDtoValidator : AbstractValidator<PaymentTransferInputDto>
    {
        public PaymentTransferInputDtoValidator()
        {
            RuleFor(x => x.AccountNum).MaximumLength(50);
            RuleFor(e => e.AccountName).MaximumLength(50);
            RuleFor(e => e.BankName).MaximumLength(100);
            RuleFor(e => e.Image).MaximumLength(100);
            RuleFor(e => e.TransferNum).MaximumLength(40);
            RuleFor(e => e.Content).MaximumLength(1000);
            //RuleFor(e => e.Amount).GreaterThanOrEqualTo(0);
            RuleFor(e => e.ReferenceBanking).MaximumLength(40);
        }
    }
}
