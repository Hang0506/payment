using FluentValidation;
using FRTTMO.PaymentCore.Dto;

namespace FRTTMO.PaymentCore.Validations.FluentValidations
{
    public class DepositByCardInputDtoValidator : AbstractValidator<DepositByCardInputDto>
    {
        public DepositByCardInputDtoValidator()
        {
            RuleFor(x => x).SetValidator(new DepositInputBaseDtoValidator());
            RuleForEach(x => x.Cards).SetValidator(new PaymentCardInputDtoValidator());
        }
    }
    public class PaymentCardInputDtoValidator : AbstractValidator<PaymentCardInputDto>
    {
        public PaymentCardInputDtoValidator()
        {
            RuleFor(x => x.CardNumber).MaximumLength(10);
            RuleFor(x => x.BankName).MaximumLength(100);
            RuleFor(x => x.CreatedBy).MaximumLength(50);
            RuleFor(e => e.Amount).NotNull().GreaterThanOrEqualTo(0m);
        }
    }
    public class DepositByCODInputDtoValidator : AbstractValidator<DepositByCODInputDto>
    {
        public DepositByCODInputDtoValidator()
        {
            RuleFor(x => x).SetValidator(new DepositInputBaseDtoValidator());
            RuleFor(e => e.COD).SetValidator(new PaymentCODInputDtoValidator());
        }
    }
    public class PaymentCODInputDtoValidator : AbstractValidator<PaymentCODInputDto>
    {
        public PaymentCODInputDtoValidator()
        {
            RuleFor(x => x.TransporterName).MaximumLength(50);
            RuleFor(e => e.Amount).NotNull().GreaterThanOrEqualTo(0m);
            RuleFor(e => e.waybillnumber).MaximumLength(50);
            RuleFor(e => e.CreatedBy).MaximumLength(50);
        }
    }
    public class DepositByTransferInputDtoValidator : AbstractValidator<DepositByTransferInputDto>
    {
        public DepositByTransferInputDtoValidator()
        {
            RuleFor(x => x).SetValidator(new DepositInputBaseDtoValidator());
            RuleForEach(e => e.Transfers).SetValidator(new PaymentTransferInputDtoValidator());
        }
    }
    public class DepositByCashInputDtoValidator : AbstractValidator<DepositByCashInputDto>
    {
        public DepositByCashInputDtoValidator()
        {
            RuleFor(x => x).SetValidator(new DepositInputBaseDtoValidator());
        }
    }
    public class DepositByEWalletOnlineInputDtoValidator : AbstractValidator<DepositByEWalletOnlineInputDto>
    {
        public DepositByEWalletOnlineInputDtoValidator()
        {
            RuleFor(x => x).SetValidator(new DepositInputBaseDtoValidator());
            RuleFor(x => x.EWallet).SetValidator(new PaymentEWalletDepositInputDtoValidator());
        }
    }
}
