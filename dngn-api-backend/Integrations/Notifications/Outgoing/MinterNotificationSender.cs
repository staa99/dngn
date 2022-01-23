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
        private readonly IConnectionFactory _connectionFactory;
        private readonly ILogger<MinterNotificationSender> _logger;
        private readonly RSA _rsa;
        private IConnection Connection { get; set; }
        private IModel Channel { get; set; }

        private const string QueueName = "dngn.deposits.completed";

        public MinterNotificationSender(ILogger<MinterNotificationSender> logger, IConfiguration configuration)
        {
            _logger             = logger;
            _connectionFactory = new ConnectionFactory
            {
                Uri = new Uri(configuration.GetConnectionString("RabbitMQ"))
            };
            Connection = _connectionFactory.CreateConnection();
            Channel     = Connection.CreateModel();
            
            _rsa = new RSACryptoServiceProvider();
            _rsa.ImportRSAPublicKey(Convert.FromBase64String(configuration["M2MCrypt:MinterPublicKey"]), out var _);
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
            Channel.QueueDeclare(QueueName, autoDelete: false, durable: true);
        }

        public Task SendNotification(MinterInstruction instruction)
        {
            EnsureInitialized();
            var instructionAsJson = JsonSerializer.Serialize(instruction);
            var encrypted = _rsa.Encrypt(Encoding.UTF8.GetBytes(instructionAsJson), RSAEncryptionPadding.Pkcs1);
            Channel.BasicPublish("", QueueName, mandatory: true, body: encrypted);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Channel.Close();
            Connection.Close();
            
            Channel.Dispose();
            Connection.Dispose();
        }
    }
}