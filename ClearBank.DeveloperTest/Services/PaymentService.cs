using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IAccountDataStore _accountDataStore;

        public PaymentService(IAccountDataStoreFactory accountDataStoreFactory)
        {
            _accountDataStore = accountDataStoreFactory.Create();
        }

        public MakePaymentResult MakePayment(MakePaymentRequest request)
        {
            var account = _accountDataStore.GetAccount(request.DebtorAccountNumber);

            var result = new MakePaymentResult { Success = true };

            switch (request.PaymentScheme)
            {
                case PaymentScheme.Bacs:
                    if (account == null)
                    {
                        result.Success = false;
                        result.FailureReason = PaymentFailureReason.AccountNotFound;
                    }
                    else if (!account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.Bacs))
                    {
                        result.Success = false;
                        result.FailureReason = PaymentFailureReason.SchemeNotAllowed;
                    }
                    break;

                case PaymentScheme.FasterPayments:
                    if (account == null)
                    {
                        result.Success = false;
                        result.FailureReason = PaymentFailureReason.AccountNotFound;
                    }
                    else if (!account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.FasterPayments))
                    {
                        result.Success = false;
                        result.FailureReason = PaymentFailureReason.SchemeNotAllowed;
                    }
                    else if (account.Balance < request.Amount)
                    {
                        result.Success = false;
                        result.FailureReason = PaymentFailureReason.InsufficientBalance;
                    }
                    break;

                case PaymentScheme.Chaps:
                    if (account == null)
                    {
                        result.Success = false;
                        result.FailureReason = PaymentFailureReason.AccountNotFound;
                    }
                    else if (!account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.Chaps))
                    {
                        result.Success = false;
                        result.FailureReason = PaymentFailureReason.SchemeNotAllowed;
                    }
                    else if (account.Status != AccountStatus.Live)
                    {
                        result.Success = false;
                        result.FailureReason = PaymentFailureReason.AccountNotLive;
                    }
                    break;
            }

            if (result.Success)
            {
                account.Balance -= request.Amount;
                _accountDataStore.UpdateAccount(account);
            }

            return result;
        }
    }
}
