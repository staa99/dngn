using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace DngnApiBackend.Integrations.Notifications.Outgoing
{
    public sealed class BlockchainNotifier : IBlockchainNotifier, IDisposable
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly ILogger<BlockchainNotifier> _logger;
        private readonly RSA _minterRsa;
        private readonly RSA _withdrawerRsa;
        private IConnection Connection { get; set; }
        private IModel Channel { get; set; }

        private const string MinterTriggerQueueName = "dngn.deposits.completed";
        private const string WithdrawalConfirmationQueueName = "dngn.withdrawals.completed";

        public BlockchainNotifier(ILogger<BlockchainNotifier> logger, IConfiguration configuration)
        {
            _logger             = logger;
            _connectionFactory = new ConnectionFactory
            {
                Uri = new Uri(configuration.GetConnectionString("RabbitMQ"))
            };
            Connection = _connectionFactory.CreateConnection();
            Channel     = Connection.CreateModel();
            
            _minterRsa = new RSACryptoServiceProvider();
            _withdrawerRsa = new RSACryptoServiceProvider();
            _minterRsa.ImportRSAPublicKey(Convert.FromBase64String(configuration["M2MCrypt:MinterPublicKey"]), out var _);
            _withdrawerRsa.ImportRSAPublicKey(Convert.FromBase64String(configuration["M2MCrypt:WithdrawerPublicKey"]), out var _);
        }

        private void EnsureInitialized()
        {
            if (!Connection.IsOpen)
            {
                Connection = _connectionFactory.CreateConnection();
            }
            
            if (Channel is {IsOpen: true})
            {
                return;
            }

            Channel = Connection.CreateModel();
            Channel.QueueDeclare(MinterTriggerQueueName, autoDelete: false, durable: true);
            Channel.QueueDeclare(WithdrawalConfirmationQueueName, autoDelete: false, durable: true);
        }

        public Task TriggerMinter(BlockchainTransactionInstruction instruction)
        {
            EnsureInitialized();
            var instructionAsJson = JsonSerializer.Serialize(instruction);
            _logger.LogTrace("MINTER_TRIGGERED: {Instruction}", instructionAsJson);
            var encrypted = _minterRsa.Encrypt(Encoding.UTF8.GetBytes(instructionAsJson), RSAEncryptionPadding.Pkcs1);
            Channel.BasicPublish("", MinterTriggerQueueName, mandatory: true, body: encrypted);
            return Task.CompletedTask;
        }

        public Task ConfirmWithdrawal(BlockchainTransactionInstruction instruction)
        {
            EnsureInitialized();
            var instructionAsJson = JsonSerializer.Serialize(instruction);
            _logger.LogTrace("WITHDRAWAL_CONFIRMED: {Instruction}", instructionAsJson);
            var encrypted = _withdrawerRsa.Encrypt(Encoding.UTF8.GetBytes(instructionAsJson), RSAEncryptionPadding.Pkcs1);
            Channel.BasicPublish("", WithdrawalConfirmationQueueName, mandatory: true, body: encrypted);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _minterRsa.Dispose();
            _withdrawerRsa.Dispose();
            
            Channel.Close();
            Connection.Close();
            
            Channel.Dispose();
            Connection.Dispose();
        }
    }
}