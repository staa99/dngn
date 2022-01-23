using System.Threading.Tasks;

namespace DngnApiBackend.Integrations.Notifications.Outgoing
{
    public interface IMinterNotificationSender
    {
        Task SendNotification(MinterInstruction instruction);
    }
}