using System;
using System.Threading.Tasks;
using DngnApiBackend.Data.Models;
using DngnApiBackend.Exceptions;
using DngnApiBackend.Integrations.Blockchain.Incoming;
using DngnApiBackend.Integrations.Models.Common;
using DngnApiBackend.Integrations.Models.InitiateTransfer;
using DngnApiBackend.Integrations.Transfers;
using DngnApiBackend.Services.Dto;
using DngnApiBackend.Services.Platform.Pricing;
using DngnApiBackend.Services.Repositories;
using Microsoft.Extensions.Logging;

namespace DngnApiBackend.Services.Platform.Withdrawals
{
    public class WithdrawalService : IWithdrawalService
    {
        private readonly ILogger<WithdrawalService> _logger;
        private readonly IPriceCalculatorService _priceCalculatorService;
        private readonly ITransferProcessor _transferProcessor;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IWithdrawalRepository _withdrawalRepository;

        public WithdrawalService(IWithdrawalRepository withdrawalRepository,
            IUserAccountRepository userAccountRepository, ITransferProcessor transferProcessor,
            IPriceCalculatorService priceCalculatorService, ILogger<WithdrawalService> logger)
        {
            _transferProcessor      = transferProcessor;
            _priceCalculatorService = priceCalculatorService;
            _withdrawalRepository   = withdrawalRepository;
            _userAccountRepository  = userAccountRepository;
            _logger                 = logger;
        }

        public async Task WithdrawAsync(BlockchainIncomingInstruction instruction)
        {
            _logger.LogInformation("WITHDRAWAL ({TxHash}): {Amount} DNGN to {Address}",
                instruction.TransactionHash, instruction.Amount, instruction.ToAddress);

            var userAccountDto = await _userAccountRepository.GetAccountAsync(instruction.ToAddress);
            if (userAccountDto == null)
            {
                _logger.LogCritical("WITHDRAWAL ({TxHash}): WITHDRAWAL_ERROR: Unregistered address {Address}",
                    instruction.TransactionHash, instruction.ToAddress);
                throw new ServiceException("UNREGISTERED_ADDRESS", "Address not registered");
            }

            if (userAccountDto.WithdrawalAccount?.Bank == null)
            {
                _logger.LogCritical("WITHDRAWAL ({TxHash}): WITHDRAWAL_ERROR: No withdrawal account for {Address}",
                    instruction.TransactionHash, instruction.ToAddress);
                throw new ServiceException("INCOMPLETE_REGISTRATION",
                    $"Withdrawal account not set for {instruction.ToAddress}");
            }

            var internalTransactionId = Guid.NewGuid();

            _logger.LogInformation("WITHDRAWAL ({TxHash}): Initializing transaction", instruction.TransactionHash);
            var transactionId = await _withdrawalRepository.CreateTransactionAsync(new CreateWithdrawalDto
            {
                Amount                    = instruction.Amount,
                Status                    = TransactionStatus.Processing,
                Provider                  = TransactionProvider.Flutterwave,
                BlockchainStatus          = TransactionBlockchainStatus.Completed,
                UserAccountId             = userAccountDto.Id,
                TotalPlatformFees         = _priceCalculatorService.CalculateDepositFees(instruction.Amount),
                BankAccountId             = userAccountDto.WithdrawalAccount.Id,
                TransactionType           = TransactionType.Withdrawal,
                BlockchainTransactionHash = instruction.TransactionHash,
                InternalTransactionId     = internalTransactionId
            });

            if (transactionId == default)
            {
                _logger.LogError(
                    "WITHDRAWAL ({TxHash}): WITHDRAWAL_ERROR: An error occurred while creating the transaction",
                    instruction.TransactionHash);
                throw new ServiceException("WITHDRAWAL_ERROR", "Failed to create the transaction");
            }

            _logger.LogInformation("WITHDRAWAL ({TxHash}): Transaction initialized, Initiating bank transfer",
                instruction.TransactionHash);
            var transferOutput = await _transferProcessor.InitiateTransferAsync(new InitiateTransferInput
            {
                Amount               = instruction.Amount / 100m,
                Currency             = Constants.CurrencyCode,
                Narration            = $"Withdrawal: {instruction.TransactionHash}",
                AccountNumber        = userAccountDto.WithdrawalAccount.AccountNumber,
                Provider             = TransactionProvider.Flutterwave,
                TransactionReference = internalTransactionId.ToString(),
                ProviderBankCode     = userAccountDto.WithdrawalAccount.Bank.Metadata[BankMetaKey.FlutterwaveCode]
            });

            _logger.LogInformation(
                "WITHDRAWAL ({TxHash}): Transfer initialized with {Status} status, Updating transaction",
                instruction.TransactionHash, transferOutput.Status);
            await _withdrawalRepository.UpdateTransactionForTransferAsync(transactionId, transferOutput);

            _logger.LogInformation("WITHDRAWAL ({TxHash}): Transaction updated", instruction.TransactionHash);
        }
    }
}