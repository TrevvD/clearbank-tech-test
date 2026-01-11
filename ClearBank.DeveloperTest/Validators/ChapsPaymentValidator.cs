using ClearBank.DeveloperTest.Types;
using FluentValidation;

namespace ClearBank.DeveloperTest.Validators;

public class ChapsPaymentValidator : AbstractValidator<PaymentValidationContext>
{
    public ChapsPaymentValidator()
    {
        RuleFor(x => x.Account)
            .NotNull()
            .WithMessage(PaymentFailureReason.AccountNotFound);

        RuleFor(x => x.Account)
            .Must(x => x.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.Chaps))
            .WithMessage(PaymentFailureReason.SchemeNotAllowed)
            .When(x => x.Account != null);

        RuleFor(x => x.Account.Status)
            .Equal(AccountStatus.Live)
            .WithMessage(PaymentFailureReason.AccountNotLive)
            .When(x => x.Account != null);
    }
}
