using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MongoDbCollectionMonitor.Clients.SlackApi;
using MongoDbCollectionMonitor.Commands.Common.Responses;

namespace MongoDbCollectionMonitor.Commands.SendSlackAlert
{
    internal class SendSlackAlertHandler : IRequestHandler<SendSlackAlertRequest, ProcessingStatusResponse>
    {
        private readonly ISlackApiClient _client;

        public SendSlackAlertHandler(ISlackApiClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<ProcessingStatusResponse> Handle(SendSlackAlertRequest request, CancellationToken cancellationToken)
        {
            var blocks = new List<object>
            {
                new
                {
                    type = "section",
                    text = new {type = "mrkdwn", text = $":skull: `{DateTime.UtcNow}`"}
                },
                new { type = "divider"},
                new
                {
                    type = "section",
                    text = new {type = "mrkdwn", text = $"*Request type*: `{request.RequestType}`"}
                },
                new
                {
                    type = "section",
                    text = new {type = "mrkdwn", text = $"*Failure reason*: `{request.FailureReason}`"}
                },
                new { type = "divider"},
                new
                {
                    type = "section",
                    text = new {type = "mrkdwn", text = "*Request data*:"}
                }
            };

            foreach (var (key, value) in request.RequestData)
            {
                blocks.Add(
                    new
                    {
                        type = "section",
                        text = new { type = "mrkdwn", text = $"• `{key}`: `{value}`" }
                    });
            }

            var content =
                new
                {
                    attachments = new List<object>
                    {
                        new
                        {
                            color = "#FF0000",
                            blocks = blocks
                        }
                    }
                };

            await _client.Send(JsonSerializer.Serialize(content), cancellationToken);

            return new ProcessingStatusResponse { FinalStep = ProcessingStep.SendSlackAlert };
        }
    }
}
