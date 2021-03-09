using System.Collections.Generic;
using MediatR;
using MongoDbCollectionMonitor.Commands.Common.Responses;

namespace MongoDbCollectionMonitor.Commands.ExtractDocumentIdentifier
{
    public abstract class ExtractDocumentIdentifierRequest : IRequest<ProcessingStatusResponse>
    {
        public abstract StudioCacheType CacheType { get; }

        public abstract string PropertyNameToBeExtracted { get; }

        public IDictionary<string, object> Values { get; set; }
    }
}
