using System.Collections.Generic;
using System.Linq;
using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Types;
using ClearBank.DeveloperTest.Validators;
using FluentValidation;

namespace ClearBank.DeveloperTest.Services
{
    public class PaymentService(IAccountDataStoreFactory accountDataStoreFactory) : IPaymentService
    {
        private readonly IAccountDataStore _accountDataStore = accountDataStoreFactory.Create();
        private readonly Dictionary<PaymentScheme, IValidator<PaymentValidationContext>> _validators = new()
        {
            { PaymentScheme.Bacs, new BacsPaymentValidator() },
            { PaymentScheme.FasterPayments, new FasterPaymentsValidator() },
            { PaymentScheme.Chaps, new ChapsPaymentValidator() }
        };

        public MakePaymentResult MakePayment(MakePaymentRequest request)
        {
            var account = _accountDataStore.GetAccount(request.DebtorAccountNumber);
            var context = new PaymentValidationContext { Account = account, Request = request };
            var validator = _validators[request.PaymentScheme];
            var validationResult = validator.Validate(context);

            var result = new MakePaymentResult
            {
                Success = validationResult.IsValid,
                FailureReason = validationResult.IsValid ? null : validationResult.Errors.First().ErrorMessage
            };

            if (!result.Success)
            {
                return result;
            }
            
            account.Balance -= request.Amount;
            _accountDataStore.UpdateAccount(account);
            return result;
        }
    }
}
