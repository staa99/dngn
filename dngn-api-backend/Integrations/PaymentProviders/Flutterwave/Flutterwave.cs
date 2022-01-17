using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using DngnApiBackend.Exceptions;
using DngnApiBackend.Integrations.BankUtilities;
using DngnApiBackend.Integrations.Models.Common;
using DngnApiBackend.Integrations.Models.CreateVirtualAccount;
using DngnApiBackend.Integrations.Models.InitiateTransfer;
using DngnApiBackend.Integrations.Models.LogVirtualAccountTransaction;
using DngnApiBackend.Integrations.Models.QueryTransfer;
using DngnApiBackend.Integrations.PaymentProviders.Flutterwave.Services.CreateVirtualAccount;
using DngnApiBackend.Integrations.PaymentProviders.Flutterwave.Services.GetBanks;
using DngnApiBackend.Integrations.PaymentProviders.Flutterwave.Services.InitiateTransfer;
using DngnApiBackend.Integrations.PaymentProviders.Flutterwave.Services.ResolveAccountName;
using DngnApiBackend.Integrations.PaymentProviders.Flutterwave.Services.Utilities;
using DngnApiBackend.Integrations.PaymentProviders.Flutterwave.Services.VerifyTransaction;
using DngnApiBackend.Integrations.Transfers;
using DngnApiBackend.Integrations.VirtualAccounts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DngnApiBackend.Integrations.PaymentProviders.Flutterwave
{
    public class Flutterwave : IBankListService, IProviderVirtualAccountCreator,
        IVirtualAccountCreditTransactionVerifier,
        IBankAccountNameResolver,
        ITransferProcessor, ITransferQueryProcessor
    {
        private const string CreateVirtualAccountNumberEndpoint = "/v3/virtual-account-numbers";
        private const string GetVirtualAccountNumberDetailsEndpoint = "/v3/virtual-account-numbers";
        private const string VerifyTransactionEndpoint = "/v3/transactions/{0}/verify";
        private const string ResolveAccountNameEndpoint = "/v3/accounts/resolve";
        private const string GetBanksEndpoint = "/v3/banks/NG";
        private const string InitiateTransferEndpoint = "/v3/transfers";
        private const string QueryTransferEndpoint = "/v3/transfers/{0}";
        private readonly HttpClient _flutterwave;
        private readonly ILogger<Flutterwave> _logger;


        public Flutterwave(HttpClient flutterwave, IOptions<FlutterwaveOptions> options, ILogger<Flutterwave> logger)
        {
            _logger      = logger;
            _flutterwave = flutterwave;
        }


        public async Task<ResolveAccountNameOutput> ResolveBankAccountNameAsync(string accountNumber, string bankCode)
        {
            var rawResponse = await SendRequestAsync(HttpMethod.Post,
                ResolveAccountNameEndpoint,
                new FlutterwaveResolveAccountNameRequest
                {
                    AccountNumber = accountNumber,
                    BankCode      = bankCode
                });

            var parsedResponse = new ParsedFlutterwaveResponse<FlutterwaveResolveAccountNameResponseData>(rawResponse);
            return new ResolveAccountNameOutput
            {
                AccountName   = parsedResponse.Data?.AccountName,
                AccountNumber = parsedResponse.Data?.AccountNumber
            };
        }


        public async Task<BankListServiceResponse> GetBanksAsync()
        {
            var rawResponse = await SendRequestAsync(HttpMethod.Get, GetBanksEndpoint);
            var parsedResponse = new ParsedFlutterwaveResponse<FlutterwaveGetBanksResponseData>(rawResponse);
            return new BankListServiceResponse
            {
                Status  = parsedResponse.IsSuccessful,
                Message = parsedResponse.Message,
                Data = parsedResponse.Data?.Select(b => new BankListServiceResponseItem
                {
                    Name    = b.Name,
                    LogoUrl = null,
                    Codes = new Dictionary<BankCodeType, string>
                    {
                        [BankCodeType.FlutterwaveCode] = b.Code
                    }
                }).ToList() ?? new List<BankListServiceResponseItem>()
            };
        }


        /// <summary>
        ///     Creates the VirtualAccount on the provider's side
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<CreateVirtualAccountOutput> CreateVirtualAccountAsync(CreateVirtualAccountInput input)
        {
            if (input.Provider != VirtualAccountProvider.Flutterwave)
            {
                throw new ServiceException("INVALID_PROVIDER",
                    $"The VirtualAccount provider specified '{input.Provider}' does not match the expected provider '{VirtualAccountProvider.Flutterwave}'");
            }

            if (string.IsNullOrEmpty(input.Email) ||
                string.IsNullOrEmpty(input.FirstName) ||
                string.IsNullOrEmpty(input.LastName) ||
                string.IsNullOrEmpty(input.BVN))
            {
                throw new UserException(
                    "ACCOUNT_DATA_REQUIRED",
                    "You must provide your email, first name, last name and BVN");
            }

            var trackingReference =
                $"{Constants.VirtualAccountReferencePrefix}-{Guid.NewGuid().ToString().Replace("-", "").ToUpperInvariant()}";

            var rawResponse = await SendRequestAsync(HttpMethod.Post,
                CreateVirtualAccountNumberEndpoint,
                new FlutterwaveCreateVirtualAccountRequest(input.Email,
                    $"{input.FirstName} {input.LastName}",
                    input.BVN,
                    trackingReference));

            var parsedResponse =
                new ParsedFlutterwaveResponse<FlutterwaveCreateVirtualAccountResponseData>(rawResponse);

            return new CreateVirtualAccountOutput
            {
                Status        = parsedResponse.IsSuccessful,
                Message       = parsedResponse.Message,
                AccountNumber = parsedResponse.Data?.AccountNumber,
                VirtualAccountId = parsedResponse.IsSuccessful
                    ? trackingReference
                    : default, // generate ID for successful account creations
                BankName = parsedResponse.Data?.BankName!
            };
        }


        public async Task<InitiateTransferOutput> InitiateTransferAsync(InitiateTransferInput input)
        {
            if (input.Provider != TransactionProvider.Flutterwave)
            {
                throw new ServiceException("INVALID_PROVIDER",
                    $"The transfer provider specified '{input.Provider}' does not match the expected provider '{TransactionProvider.Flutterwave}'");
            }

            if (string.IsNullOrEmpty(input.Currency) ||
                string.IsNullOrEmpty(input.AccountNumber) ||
                string.IsNullOrEmpty(input.TransactionReference) ||
                string.IsNullOrEmpty(input.ProviderBankCode) ||
                input.Amount <= 0)
            {
                throw new UserException("TRANSFER_DATA_REQUIRED",
                    "You must specify the recipient account number");
            }

            var rawResponse = await SendRequestAsync(HttpMethod.Post,
                InitiateTransferEndpoint,
                new FlutterwaveInitiateTransferRequest(input.ProviderBankCode, input.AccountNumber, input.Amount,
                    input.Narration, input.Currency, input.TransactionReference));

            var parsedResponse =
                new ParsedFlutterwaveResponse<FlutterwaveInitiateTransferResponseData>(rawResponse);

            return InitializeTransferOutput<InitiateTransferOutput>(parsedResponse);
        }


        public async Task<QueryTransferOutput?> QueryTransferAsync(string providerReference)
        {
            var endpoint = string.Format(QueryTransferEndpoint, providerReference);
            var rawResponse = await SendRequestAsync(HttpMethod.Get, endpoint);
            var parsedResponse =
                new ParsedFlutterwaveResponse<FlutterwaveInitiateTransferResponseData>(rawResponse);

            if (!parsedResponse.IsSuccessful ||
                parsedResponse.Data?.Id == null)
            {
                return null;
            }

            return InitializeTransferOutput<QueryTransferOutput>(parsedResponse);
        }


        public async Task<LogVirtualAccountTransactionInput?> VerifyVirtualAccountCreditTransactionAsync(
            string providerReference)
        {
            var endpoint = string.Format(VerifyTransactionEndpoint, providerReference);
            var rawResponse = await SendRequestAsync(HttpMethod.Get, endpoint);
            var parsedResponse =
                new ParsedFlutterwaveResponse<FlutterwaveVerifyTransactionResponseData>(rawResponse);

            if (!parsedResponse.IsSuccessful ||
                parsedResponse.Data?.Id == null ||
                parsedResponse.Data.Status == null ||
                parsedResponse.Data.TxRef == null ||
                parsedResponse.Data.Currency == null)
            {
                return null;
            }

            return new LogVirtualAccountTransactionInput
            {
                Amount = (long) (parsedResponse.Data.ChargedAmount * 100),
                Fees = (long) ((parsedResponse.Data.AppFee + parsedResponse.Data.MerchantFee) * 100),
                Narration = parsedResponse.Data.Narration,
                Provider = VirtualAccountProvider.Flutterwave,
                Status = parsedResponse.Data.Status,
                BankTransactionReference = parsedResponse.Data.FlwRef,
                ProviderVirtualAccountId = parsedResponse.Data.TxRef,
                ProviderTransactionReference = parsedResponse.Data.Id.ToString(),
                Currency = parsedResponse.Data.Currency
            };
        }


        private static T InitializeTransferOutput<T>(
            ParsedFlutterwaveResponse<FlutterwaveInitiateTransferResponseData> parsedResponse)
            where T : BaseTransferOutput, new()
        {
            var innerResponseStatus = parsedResponse.Data?.Status?.ToLowerInvariant();
            var status = TransactionStatusUtilities.GetTransactionStatus(parsedResponse.Status, innerResponseStatus);

            // decimal.TryParse(parsedResponse.Data?.Amount ?? "", out var amount);
            // decimal.TryParse(parsedResponse.Data?.Fee ?? "", out var fee);

            return new T
            {
                Status = status,
                StatusMessage = string.IsNullOrWhiteSpace(parsedResponse.Data?.CompleteMessage)
                    ? parsedResponse.Message
                    : parsedResponse.Data.CompleteMessage,
                Amount                       = (long) ((parsedResponse.Data?.Amount ?? 0) * 100),
                TransferFees                 = (long) ((parsedResponse.Data?.Fee ?? 0) * 100),
                AccountNumber                = parsedResponse.Data?.AccountNumber,
                ProviderBankReference        = null,
                ProviderTransactionReference = parsedResponse.Data?.Id.ToString()
            };
        }


        private static FlutterwaveResponse ParseJsonDocumentToFlutterwaveResponse(JsonElement decryptedJson)
        {
            string? status = null;
            string? message = null;
            string? data = null;

            foreach (var property in decryptedJson.EnumerateObject())
            {
                switch (property.Name.ToLowerInvariant())
                {
                    case "status":
                        status = property.Value.GetString();
                        break;
                    case "message":
                        message = property.Value.GetString();
                        break;
                    case "data":
                        data = property.Value.GetRawText();
                        break;
                }
            }

            if (status == null || message == null)
            {
                throw new ServiceException("INVALID_TPP_DATA",
                    "Received an invalid response from the upstream provider");
            }

            return new FlutterwaveResponse(status, message, data);
        }


        private async Task<FlutterwaveResponse> SendRequestAsync(HttpMethod method,
            string endpoint,
            object? data = null)
        {
            // todo: log API calls and responses
            var message = new HttpRequestMessage(method, endpoint);
            if (data != null)
            {
                message.Content = new StringContent(JsonSerializer.Serialize(data))
                {
                    Headers =
                    {
                        ContentType = new MediaTypeHeaderValue("application/json")
                    }
                };
            }

            var response = await _flutterwave.SendAsync(message);
            var responseJsonString = await response.Content.ReadAsStringAsync();
            var responseJson = JsonDocument.Parse(responseJsonString).RootElement;
            return ParseJsonDocumentToFlutterwaveResponse(responseJson);
        }
    }
}