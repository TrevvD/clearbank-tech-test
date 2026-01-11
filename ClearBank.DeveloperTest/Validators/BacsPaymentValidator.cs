using ClearBank.DeveloperTest.Types;
using FluentValidation;

namespace ClearBank.DeveloperTest.Validators;

public class BacsPaymentValidator : AbstractValidator<PaymentValidationContext>
{
    public BacsPaymentValidator()
    {
        RuleFor(x => x.Account)
            .NotNull()
            .WithMessage(PaymentFailureReason.AccountNotFound);

        RuleFor(x => x.Account)
            .Must(x => x.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.Bacs))
            .WithMessage(PaymentFailureReason.SchemeNotAllowed)
            .When(x => x.Account != null);
    }
}
