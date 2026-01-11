namespace ClearBank.DeveloperTest.Types;

public static class PaymentFailureReason
{
    public const string AccountNotFound = "Account not found";
    public const string SchemeNotAllowed = "Payment scheme not allowed";
    public const string InsufficientBalance = "Insufficient balance";
    public const string AccountNotLive = "Account is not in Live status";
}
