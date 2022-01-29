using System;
using System.Threading.Tasks;
using DngnApiBackend.Integrations.Models.Common;
using DngnApiBackend.Integrations.Models.WebHooks;
using DngnApiBackend.Integrations.Transfers;
using DngnApiBackend.Services.Repositories;
using DngnApiBackend.Services.WebhookServices;
using Microsoft.Extensions.Logging;

namespace DngnApiBackend.Integrations.PaymentProviders.Flutterwave.Services.WebHooks
{
    // todo: Implement customer notifications
    public class FlutterwaveOutwardBankTransferHook : IOutwardBankTransferHook
    {
        private readonly ILogger<FlutterwaveOutwardBankTransferHook> _logger;
        private readonly ITransferQueryProcessor _transferQueryProcessor;
        private readonly IWithdrawalRepository _withdrawalRepository;


        public FlutterwaveOutwardBankTransferHook(IWithdrawalRepository withdrawalRepository,
            ITransferQueryProcessor transferQueryProcessor,
            ILogger<FlutterwaveOutwardBankTransferHook> logger)
        {
            _withdrawalRepository   = withdrawalRepository;
            _transferQueryProcessor = transferQueryProcessor;
            _logger                 = logger;
        }


        public async Task ProcessHookAsync(JsonWebhookModel model)
        {
            if (model is not FlutterwaveJsonWebhookModel flwModel)
            {
                _logger.LogInformation(
                    "OutwardBankTransferHook: Invalid attempt to process a non-flutterwave webhook model by a flutterwave webhook processor");

                return;
            }

            var providerReference = flwModel.Id;
            _logger.LogInformation("OutwardBankTransferHook: A notification is being processed. Reference: {Reference}",
                providerReference);

            // load the transaction data from flutterwave
            var data = await _transferQueryProcessor.QueryTransferAsync(providerReference);
            if (data?.ProviderTransactionReference == null)
            {
                _logger.LogInformation(
                    "OutwardBankTransferHook: The provider failed to resolve the reference '{Reference}'",
                    providerReference);

                return;
            }

            try
            {
                var newStatus = data.Status;
                var transaction =
                    await _withdrawalRepository.GetTransactionByProviderReferenceAsync(
                        TransactionProvider.Flutterwave, providerReference);

                if (transaction == null)
                {
                    _logger.LogCritical(
                        "The transaction does not exist. Provider reference: {Reference}, New status: {Status}",
                        providerReference, newStatus);
                    return;
                }

                await _withdrawalRepository.UpdateTransactionStatusAsync(transaction.Id, newStatus, data.TransferFees);
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    "An error occurred while processing the webhook. Message: {Message}, StackTrace: {StackTrace}",
                    e.Message, e.StackTrace);

                throw;
            }
        }
    }
}