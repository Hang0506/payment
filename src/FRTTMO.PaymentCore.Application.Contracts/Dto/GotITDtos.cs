namespace FRTTMO.PaymentCore.Dto
{
    public interface IValidatedVoucher
    {
        bool IsValidated { get; }
    }
    public interface IUseVoucherStatus
    {
        bool UseVoucherSuccessed { get; }
    }
}
