namespace ClearBank.DeveloperTest.Types
{
    public class MakePaymentResult
    {
        public bool Success { get; set; }
        public PaymentFailureReason FailureReason { get; set; }
    }
}
