using System;
using System.Threading.Tasks;
using DngnApiBackend.Exceptions;
using DngnApiBackend.Integrations.PaymentProviders.Flutterwave;
using DngnApiBackend.Integrations.PaymentProviders.Flutterwave.Services.WebHooks;
using DngnApiBackend.Services.WebhookServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DngnApiBackend.Controllers.WebHooks
{
    [AllowAnonymous]
    [Route("hooks/flutterwave")]
    public class FlutterwaveHook : BaseController
    {
        private readonly ILogger<FlutterwaveHook> _logger;
        private readonly IInwardBankTransferHook _inwardBankTransferHook;
        private readonly IOutwardBankTransferHook _outwardBankTransferHook;
        private readonly FlutterwaveOptions _options;

        public FlutterwaveHook(ILogger<FlutterwaveHook> logger, IOptions<FlutterwaveOptions> options, IInwardBankTransferHook inwardBankTransferHook, IOutwardBankTransferHook outwardBankTransferHook)
        {
            _logger                       = logger;
            _inwardBankTransferHook       = inwardBankTransferHook;
            _outwardBankTransferHook = outwardBankTransferHook;
            _options                      = options.Value;
        }
        
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            // todo: For max efficiency, dump the webhook call on a queue to be processed internally later
            var request = HttpContext.Request;
            try
            {
                if (!request.Headers.TryGetValue("verif-hash", out var hash) || hash != _options.SecretHash)
                {
                    _logger.LogCritical("Attempt to trigger flutterwave webhook with wrong hash");
                    
                    // return a 404 for misdirection
                    return NotFound();
                }

                var model = new FlutterwaveJsonWebhookModel(request, _logger);
                await model.InitializeAsync();

                var eventName = model.EventName.ToLowerInvariant();
                var eventType = model.EventType.ToLowerInvariant();

                switch (eventName)
                {
                    case "charge.completed" when eventType == "bank_transfer_transaction":
                        await _inwardBankTransferHook.ProcessHookAsync(model);
                        break;
                    case "transfer.completed" when eventType == "transfer":
                        await _outwardBankTransferHook.ProcessHookAsync(model);
                        break;
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "An unknown error occurred while processing the request.\n\n" +
                    "Message: {Message}\n\n" +
                    "Stack Trace: {StackTrace}",
                    ex.Message, ex.StackTrace);

                throw new ServiceException("UNKNOWN_WEBHOOK_ERROR", "An error occurred while processing the webhook");
            }
        }
    }
}