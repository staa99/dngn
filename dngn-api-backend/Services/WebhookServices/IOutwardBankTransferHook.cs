using System.Threading.Tasks;
using DngnApiBackend.Integrations.Models.WebHooks;

namespace DngnApiBackend.Services.WebhookServices
{
    public interface IOutwardBankTransferHook
    {
        Task ProcessHookAsync(JsonWebhookModel model);
    }
}