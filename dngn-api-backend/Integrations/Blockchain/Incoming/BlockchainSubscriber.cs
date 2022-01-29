using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DngnApiBackend.Services.Platform.Withdrawals;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DngnApiBackend.Integrations.Blockchain.Incoming
{
    public class BlockchainSubscriber: BackgroundService
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BlockchainSubscriber> _logger;
        private readonly RSA _rsa;
        private IConnection Connection { get; set; } = null!;
        private IModel Channel { get; set; } = null!;

        private const string MinterCallbackQueueName = "dngn.deposits.callback";
        private const string WithdrawalTriggerQueueName = "dngn.withdrawals.initiated";

        public BlockchainSubscriber(IServiceProvider serviceProvider, ILogger<BlockchainSubscriber> logger, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger               = logger;
            _connectionFactory = new ConnectionFactory
            {
                Uri = new Uri(configuration.GetConnectionString("RabbitMQ"))
            };
            
            _rsa     = new RSACryptoServiceProvider();
            _rsa.ImportRSAPrivateKey(Convert.FromBase64String(configuration["M2MCrypt:CoreAPIPrivateKey"]), out var _);
            EnsureInitialized();
        }
        
        private void EnsureInitialized()
        {
            if (Connection is not {IsOpen: true})
            {
                Connection = _connectionFactory.CreateConnection();
            }
            
            if (Channel is {IsOpen: true})
            {
                return;
            }

            Channel = Connection.CreateModel();
            Channel.QueueDeclare(MinterCallbackQueueName, autoDelete: false, durable: true, exclusive: false);
            Channel.QueueDeclare(WithdrawalTriggerQueueName, autoDelete: false, durable: true, exclusive: false);
        }
        
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                Dispose();
                return Task.CompletedTask;
            }

            EnsureInitialized();
            var minterCallbackConsumer = new EventingBasicConsumer(Channel);
            var withdrawerTriggerConsumer = new EventingBasicConsumer(Channel);
            
            minterCallbackConsumer.Received += OnMinterCallbackMessage;
            withdrawerTriggerConsumer.Received += OnWithdrawerTriggerMessage;

            Channel.BasicConsume(MinterCallbackQueueName, false, minterCallbackConsumer);
            Channel.BasicConsume(WithdrawalTriggerQueueName, false, withdrawerTriggerConsumer);
            return Task.CompletedTask;
        }

        private async void OnWithdrawerTriggerMessage(object? sender, BasicDeliverEventArgs e)
        {
            _logger.LogTrace("WITHDRAWAL: NEW_TRIGGER");
            if (sender is not EventingBasicConsumer consumer)
            {
                _logger.LogError("WITHDRAWAL: FAILED_INVALID_PARAMETERS");
                Channel.BasicReject(e.DeliveryTag, false);
                return;
            }
            
            try
            {
                _logger.LogTrace("WITHDRAWAL: DECRYPTING_BODY");
                var message = _rsa.Decrypt(e.Body.ToArray(), RSAEncryptionPadding.Pkcs1);
                _logger.LogTrace("WITHDRAWAL: DESERIALIZING_MESSAGE");
                var instruction = JsonSerializer.Deserialize<BlockchainIncomingInstruction>(message);
                
                if (instruction == null)
                {
                    _logger.LogError("WITHDRAWAL: DESERIALIZATION_FAILED. UTF-8 MESSAGE: {Message}", Encoding.UTF8.GetString(message));
                    consumer.Model.BasicReject(e.DeliveryTag, false);
                    return;
                }
                
                await using var scope = _serviceProvider.CreateAsyncScope();
                var withdrawalService = scope.ServiceProvider.GetRequiredService<IWithdrawalService>();
                
                _logger.LogInformation("WITHDRAWAL: PROCESSING");
                await withdrawalService.WithdrawAsync(instruction);
                _logger.LogInformation("WITHDRAWAL: PROCESSING_COMPLETE");
                consumer.Model.BasicAck(e.DeliveryTag, false);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "WITHDRAWAL: FAILED_ERROR_OCCURRED");
                Console.WriteLine(exception);
                consumer.Model.BasicReject(e.DeliveryTag, false);
            }
        }

        private void OnMinterCallbackMessage(object? sender, BasicDeliverEventArgs e)
        {
            throw new NotImplementedException();
        }

        public sealed override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            base.Dispose();
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            _rsa.Dispose();
            Channel.Close();
            Connection.Close();
            Connection.Dispose();
            Channel.Dispose();
        }
    }
}