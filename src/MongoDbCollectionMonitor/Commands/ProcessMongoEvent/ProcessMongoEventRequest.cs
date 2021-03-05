using System.Collections.Generic;
using MediatR;
using MongoDbCollectionMonitor.Commands.Common.Responses;

namespace MongoDbCollectionMonitor.Commands.ProcessMongoEvent
{
    public class ProcessMongoEventRequest : IRequest<ProcessingStatusResponse>
    {
        public string CollectionName  { get; set; }

        public string OperationName { get; set; }

        public IDictionary<string, object> Values { get; set; }
    }
}
