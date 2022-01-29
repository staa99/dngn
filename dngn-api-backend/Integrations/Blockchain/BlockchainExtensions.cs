using DngnApiBackend.Integrations.Blockchain.Incoming;
using DngnApiBackend.Integrations.Blockchain.Outgoing;
using Microsoft.Extensions.DependencyInjection;

namespace DngnApiBackend.Integrations.Blockchain
{
    public static class NotificationExtensions
    {
        public static IServiceCollection AddNotifications(this IServiceCollection services) =>
            services.AddSingleton<IBlockchainNotifier, BlockchainNotifier>()
                .AddHostedService<BlockchainSubscriber>();
    }
}