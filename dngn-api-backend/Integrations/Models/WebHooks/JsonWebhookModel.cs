using System;
using System.Text.Json;
using System.Threading.Tasks;
using DngnApiBackend.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DngnApiBackend.Integrations.Models.WebHooks
{
    public class JsonWebhookModel
    {
        private readonly ILogger _logger;
        private readonly HttpRequest _request;


        public JsonWebhookModel(HttpRequest request, ILogger logger)
        {
            if (request.Body == null || request.ContentType == null)
            {
                throw new UserException("INVALID_REQUEST", "The request must have a request body");
            }

            if (!request.ContentType.Equals("text/json", StringComparison.OrdinalIgnoreCase) &&
                !request.ContentType.Equals("application/json", StringComparison.OrdinalIgnoreCase))
            {
                throw new UserException("INVALID_REQUEST",
                    "The content type must be 'text/json' or 'application/json'");
            }

            _request = request;
            _logger  = logger;
        }


        public string EventName { get; set; } = null!;
        public string EventType { get; set; } = null!;
        public JsonElement JsonRoot { get; set; }


        public virtual async Task InitializeAsync()
        {
            try
            {
                var document = await JsonDocument.ParseAsync(_request.Body);
                JsonRoot = document.RootElement;
            }
            catch (JsonException)
            {
                _logger.LogWarning("An error occured while parsing the request body");
                throw new UserException("INVALID_REQUEST", "The request body must be valid json");
            }
        }
    }
}