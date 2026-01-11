using AutoFixture;
using AutoFixture.AutoMoq;
using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Services;
using ClearBank.DeveloperTest.Types;
using FluentAssertions;
using Moq;
using Xunit;

namespace ClearBank.DeveloperTest.Tests;

public class PaymentServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IAccountDataStore> _dataStoreMock;
    private readonly PaymentService _sut;

    public PaymentServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _dataStoreMock = new Mock<IAccountDataStore>();

        var factoryMock = new Mock<IAccountDataStoreFactory>();
        factoryMock.Setup(f => f.Create()).Returns(_dataStoreMock.Object);
        _sut = new PaymentService(factoryMock.Object);
    }

    [Theory]
    [InlineData(PaymentScheme.Bacs)]
    [InlineData(PaymentScheme.FasterPayments)]
    [InlineData(PaymentScheme.Chaps)]
    public void MakePayment_AccountIsNull_ReturnsAccountNotFound(PaymentScheme scheme)
    {
        var request = CreateRequest(scheme);
        SetupNullAccount(request.DebtorAccountNumber);

        var result = _sut.MakePayment(request);

        result.Success.Should().BeFalse();
        result.FailureReason.Should().Be(PaymentFailureReason.AccountNotFound);
    }

    [Theory]
    [InlineData(PaymentScheme.Bacs, AllowedPaymentSchemes.FasterPayments)]
    [InlineData(PaymentScheme.FasterPayments, AllowedPaymentSchemes.Bacs)]
    [InlineData(PaymentScheme.Chaps, AllowedPaymentSchemes.Bacs)]
    public void MakePayment_SchemeNotAllowed_ReturnsSchemeNotAllowed(PaymentScheme scheme, AllowedPaymentSchemes allowedSchemes)
    {
        var request = CreateRequest(scheme);
        var account = CreateAccount(allowedSchemes);
        SetupAccount(request.DebtorAccountNumber, account);

        var result = _sut.MakePayment(request);

        result.Success.Should().BeFalse();
        result.FailureReason.Should().Be(PaymentFailureReason.SchemeNotAllowed);
    }

    [Theory]
    [InlineData(PaymentScheme.Bacs, AllowedPaymentSchemes.Bacs)]
    [InlineData(PaymentScheme.FasterPayments, AllowedPaymentSchemes.FasterPayments)]
    [InlineData(PaymentScheme.Chaps, AllowedPaymentSchemes.Chaps)]
    public void MakePayment_SchemeAllowed_ReturnsSuccess(PaymentScheme scheme, AllowedPaymentSchemes allowedSchemes)
    {
        var request = CreateRequest(scheme);
        var account = CreateAccount(allowedSchemes);
        SetupAccount(request.DebtorAccountNumber, account);

        var result = _sut.MakePayment(request);

        result.Success.Should().BeTrue();
        result.FailureReason.Should().BeNull();
    }

    [Fact]
    public void MakePayment_FasterPayments_InsufficientBalance_ReturnsInsufficientBalance()
    {
        var request = CreateRequest(PaymentScheme.FasterPayments, 100m);
        var account = CreateAccount(AllowedPaymentSchemes.FasterPayments, balance: 50m);
        SetupAccount(request.DebtorAccountNumber, account);

        var result = _sut.MakePayment(request);

        result.Success.Should().BeFalse();
        result.FailureReason.Should().Be(PaymentFailureReason.InsufficientBalance);
    }

    [Theory]
    [InlineData(100, 200)]
    [InlineData(100, 100)]
    public void MakePayment_FasterPayments_SufficientBalance_ReturnsSuccess(decimal amount, decimal balance)
    {
        var request = CreateRequest(PaymentScheme.FasterPayments, amount);
        var account = CreateAccount(AllowedPaymentSchemes.FasterPayments, balance);
        SetupAccount(request.DebtorAccountNumber, account);

        var result = _sut.MakePayment(request);

        result.Success.Should().BeTrue();
        result.FailureReason.Should().BeNull();
    }

    [Theory]
    [InlineData(AccountStatus.Disabled)]
    [InlineData(AccountStatus.InboundPaymentsOnly)]
    public void MakePayment_Chaps_AccountNotLive_ReturnsAccountNotLive(AccountStatus status)
    {
        var request = CreateRequest(PaymentScheme.Chaps);
        var account = CreateAccount(AllowedPaymentSchemes.Chaps, status: status);
        SetupAccount(request.DebtorAccountNumber, account);

        var result = _sut.MakePayment(request);

        result.Success.Should().BeFalse();
        result.FailureReason.Should().Be(PaymentFailureReason.AccountNotLive);
    }

    [Fact]
    public void MakePayment_Success_DeductsBalanceFromAccount()
    {
        var request = CreateRequest(PaymentScheme.Bacs, 50m);
        var account = CreateAccount(AllowedPaymentSchemes.Bacs, balance: 200m);
        SetupAccount(request.DebtorAccountNumber, account);

        _sut.MakePayment(request);

        account.Balance.Should().Be(150m);
    }

    [Fact]
    public void MakePayment_Success_CallsUpdateAccount()
    {
        var request = CreateRequest(PaymentScheme.Bacs);
        var account = CreateAccount(AllowedPaymentSchemes.Bacs);
        SetupAccount(request.DebtorAccountNumber, account);

        _sut.MakePayment(request);

        _dataStoreMock.Verify(x => x.UpdateAccount(account), Times.Once);
    }

    [Fact]
    public void MakePayment_Failure_DoesNotUpdateAccount()
    {
        var request = CreateRequest(PaymentScheme.Bacs);
        SetupNullAccount(request.DebtorAccountNumber);

        _sut.MakePayment(request);

        _dataStoreMock.Verify(x => x.UpdateAccount(It.IsAny<Account>()), Times.Never);
    }

    private MakePaymentRequest CreateRequest(PaymentScheme scheme, decimal? amount = null)
    {
        var request = _fixture.Create<MakePaymentRequest>();
        request.PaymentScheme = scheme;
        if (amount.HasValue) request.Amount = amount.Value;
        return request;
    }

    private Account CreateAccount(AllowedPaymentSchemes schemes, decimal balance = 1000m, AccountStatus status = AccountStatus.Live)
    {
        var account = _fixture.Create<Account>();
        account.AllowedPaymentSchemes = schemes;
        account.Balance = balance;
        account.Status = status;
        return account;
    }

    private void SetupAccount(string accountNumber, Account account) =>
        _dataStoreMock.Setup(x => x.GetAccount(accountNumber)).Returns(account);

    private void SetupNullAccount(string accountNumber) =>
        _dataStoreMock.Setup(x => x.GetAccount(accountNumber)).Returns((Account)null);
}
