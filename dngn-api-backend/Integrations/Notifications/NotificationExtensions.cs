using DngnApiBackend.Integrations.Notifications.Outgoing;
using Microsoft.Extensions.DependencyInjection;

namespace DngnApiBackend.Integrations.Notifications
{
    public static class NotificationExtensions
    {
        public static IServiceCollection AddNotifications(this IServiceCollection services) =>
            services.AddSingleton<IMinterNotificationSender, MinterNotificationSender>();
    }
}