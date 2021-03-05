using System.Collections.Generic;
using MediatR;
using MongoDbCollectionMonitor.Commands.Common.Responses;

namespace MongoDbCollectionMonitor.Commands.ResolveCollectionType
{
    public class ResolveCollectionTypeRequest : IRequest<ProcessingStatusResponse>
    {
        public string AssemblyName { get; set; }

        public string HandlerRequestFullyQualifiedName { get; set; }

        public IDictionary<string, object> Values { get; set; }
    }
}
