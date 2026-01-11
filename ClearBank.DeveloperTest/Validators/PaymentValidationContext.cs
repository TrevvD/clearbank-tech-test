using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Validators;

public class PaymentValidationContext
{
    public Account Account { get; set; }
    public MakePaymentRequest Request { get; set; }
}