using System.Threading.Tasks;
using DngnApiBackend.Integrations.Models.WebHooks;

namespace DngnApiBackend.Services.WebhookServices
{
    public interface IInwardBankTransferHook
    {
        Task ProcessHookAsync(JsonWebhookModel model);
    }
}