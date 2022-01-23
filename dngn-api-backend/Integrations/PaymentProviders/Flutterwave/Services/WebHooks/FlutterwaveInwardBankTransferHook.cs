using System;
using System.Threading.Tasks;
using DngnApiBackend.Data.Models;
using DngnApiBackend.Integrations.Models.Common;
using DngnApiBackend.Integrations.Models.WebHooks;
using DngnApiBackend.Integrations.VirtualAccounts;
using DngnApiBackend.Services.Dto;
using DngnApiBackend.Services.Platform.Pricing;
using DngnApiBackend.Services.Repositories;
using DngnApiBackend.Services.WebhookServices;
using Microsoft.Extensions.Logging;

// ReSharper disable SwitchStatementMissingSomeEnumCasesNoDefault


namespace DngnApiBackend.Integrations.PaymentProviders.Flutterwave.Services.WebHooks
{
    // todo: Implement customer notifications
    public class FlutterwaveInwardBankTransferHook : IInwardBankTransferHook
    {
        private readonly IBankAccountRepository _bankAccountRepository;
        private readonly IVirtualAccountCreditTransactionVerifier _creditTransactionVerifier;
        private readonly IDepositRepository _depositRepository;
        private readonly ILogger<FlutterwaveInwardBankTransferHook> _logger;
        private readonly IPriceCalculatorService _priceCalculatorService;


        public FlutterwaveInwardBankTransferHook(IDepositRepository depositRepository,
            IBankAccountRepository bankAccountRepository,
            IVirtualAccountCreditTransactionVerifier creditTransactionVerifier,
            IPriceCalculatorService priceCalculatorService,
            ILogger<FlutterwaveInwardBankTransferHook> logger)
        {
            _depositRepository         = depositRepository;
            _bankAccountRepository     = bankAccountRepository;
            _creditTransactionVerifier = creditTransactionVerifier;
            _priceCalculatorService    = priceCalculatorService;
            _logger                    = logger;
        }


        public async Task ProcessHookAsync(JsonWebhookModel model)
        {
            if (model is not FlutterwaveJsonWebhookModel flwModel)
            {
                _logger.LogInformation(
                    "InwardBankTransferHook: Invalid attempt to process a non-flutterwave webhook model by a flutterwave webhook processor");

                return;
            }

            var providerReference = flwModel.Id;
            _logger.LogInformation("InwardBankTransferHook: A notification is being processed. Reference: {Reference}",
                providerReference);

            // load the transaction data from flutterwave
            var data = await _creditTransactionVerifier.VerifyVirtualAccountCreditTransactionAsync(providerReference);
            if (data == null)
            {
                _logger.LogInformation(
                    "InwardBankTransferHook: The provider failed to resolve the reference '{Reference}'",
                    providerReference);

                return;
            }

            try
            {
                if (!data.IsSuccessful)
                {
                    _logger.LogError("Transaction failed: {Reference}", providerReference);
                    return;
                }

                var newStatus = data.IsSuccessful ? TransactionStatus.Successful : TransactionStatus.Failed;
                var transaction =
                    await _depositRepository.GetTransactionByProviderReferenceAsync(TransactionProvider.Flutterwave,
                        providerReference);
                if (transaction != null)
                {
                    _logger.LogError(
                        "The transaction has already been processed. Provider reference: {Reference}, Current status: {CurrentStatus}, New status: {NewStatus}",
                        providerReference, transaction.Status, newStatus);
                    return;
                }

                var bankAccount = await _bankAccountRepository.GetBankAccountAsync(TransactionProvider.Flutterwave,
                    data.ProviderVirtualAccountId);

                if (bankAccount == null)
                {
                    _logger.LogCritical(
                        "The transaction refers to an untracked bank account. " +
                        "Provider Transaction Reference: {TransactionReference}, " +
                        "Provider Bank Account Reference: {BankAccountReference}",
                        providerReference, data.ProviderVirtualAccountId);
                    return;
                }

                await _depositRepository.CreateTransactionAsync(new CreateDepositDto
                {
                    Amount                = data.Amount,
                    ProviderFees          = data.Fees,
                    TotalPlatformFees     = _priceCalculatorService.CalculateDepositFees(data.Amount),
                    Status                = TransactionStatus.Successful,
                    TransactionType       = TransactionType.Deposit,
                    BankTransactionId     = data.BankTransactionReference,
                    ProviderTransactionId = data.ProviderTransactionReference,
                    InternalTransactionId = Guid.NewGuid(),
                    Provider              = TransactionProvider.Flutterwave,
                    BankAccountId         = bankAccount.Id,
                    UserAccountId         = bankAccount.UserId,
                    RawWebhookPayload     = model.JsonRoot.ToString()
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    "An error occurred while processing the webhook. Transaction Reference: {Reference}, Message: {Message}, StackTrace: {StackTrace}",
                    providerReference, e.Message, e.StackTrace);

                throw;
            }
        }
    }
}