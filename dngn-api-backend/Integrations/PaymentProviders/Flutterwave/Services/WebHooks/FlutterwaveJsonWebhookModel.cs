using System.Threading.Tasks;
using DngnApiBackend.Integrations.Models.WebHooks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DngnApiBackend.Integrations.PaymentProviders.Flutterwave.Services.WebHooks
{
    public class FlutterwaveJsonWebhookModel : JsonWebhookModel
    {
        public FlutterwaveJsonWebhookModel(HttpRequest request, ILogger logger) : base(request, logger)
        {
        }


        public string Id { get; set; } = null!;
        public string Status { get; set; } = null!;


        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            var root = JsonRoot;
            var data = root.GetProperty("data");

            EventName = root.GetProperty("event").GetString()!;
            EventType = root.GetProperty("event.type").GetString()!;
            Id        = data.GetProperty("id").GetInt64().ToString();
            Status    = data.GetProperty("status").GetString()!;
        }
    }
}