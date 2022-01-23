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
    public sealed class MinterNotificationSender : IMinterNotificationSender, IDisposable
    {
        private readonly ILogger<MinterNotificationSender> _logger;
        private readonly IConnection _connection;
        private readonly RSA _rsa;
        private IModel Channel { get; set; }

        private const string QueueName = "dngn.deposits.completed";

        public MinterNotificationSender(ILogger<MinterNotificationSender> logger, IConfiguration configuration)
        {
            _logger             = logger;
            var connectionFactory = new ConnectionFactory
            {
                Uri = new Uri(configuration.GetConnectionString("RabbitMQ"))
            };
            _connection = connectionFactory.CreateConnection();
            Channel     = _connection.CreateModel();
            
            _rsa = new RSACryptoServiceProvider();
            _rsa.ImportRSAPublicKey(Encoding.UTF8.GetBytes(configuration["M2MCrypt:MinterPublicKey"]), out var _);
        }

        private void EnsureInitialized()
        {
            if (Channel is {IsOpen: true})
            {
                return;
            }

            Channel = _connection.CreateModel();
            Channel.QueueDeclare(QueueName, autoDelete: false, durable: true);
        }

        public Task SendNotification(MinterInstruction instruction)
        {
            EnsureInitialized();
            var instructionAsJson = JsonSerializer.Serialize(instruction);
            var encrypted = _rsa.Encrypt(Encoding.UTF8.GetBytes(instructionAsJson), RSAEncryptionPadding.OaepSHA256);
            Channel.BasicPublish("", QueueName, mandatory: true, body: encrypted);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Channel.Close();
            _connection.Close();
            
            Channel.Dispose();
            _connection.Dispose();
        }
    }
}