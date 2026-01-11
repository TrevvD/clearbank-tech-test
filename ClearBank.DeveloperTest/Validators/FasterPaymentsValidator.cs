using ClearBank.DeveloperTest.Types;
using FluentValidation;

namespace ClearBank.DeveloperTest.Validators;

public class FasterPaymentsValidator : AbstractValidator<PaymentValidationContext>
{
    public FasterPaymentsValidator()
    {
        RuleFor(x => x.Account)
            .NotNull()
            .WithMessage(PaymentFailureReason.AccountNotFound);

        RuleFor(x => x.Account)
            .Must(x => x.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.FasterPayments))
            .WithMessage(PaymentFailureReason.SchemeNotAllowed)
            .When(x => x.Account != null);

        RuleFor(x => x.Account.Balance)
            .GreaterThanOrEqualTo(x => x.Request.Amount)
            .WithMessage(PaymentFailureReason.InsufficientBalance)
            .When(x => x.Account != null);
    }
}
