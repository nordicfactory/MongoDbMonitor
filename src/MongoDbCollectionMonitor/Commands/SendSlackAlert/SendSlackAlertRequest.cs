using System.Collections.Generic;
using MediatR;
using MongoDbCollectionMonitor.Commands.Common.Responses;

namespace MongoDbCollectionMonitor.Commands.SendSlackAlert
{
    public class SendSlackAlertRequest : IRequest<ProcessingStatusResponse>
    {
        public string RequestType { get; set; }

        public string FailureReason { get; set; }

        public IDictionary<string, object> RequestData { get; set; }
    }
}
