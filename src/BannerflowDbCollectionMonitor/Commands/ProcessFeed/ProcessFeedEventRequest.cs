using MongoDbCollectionMonitor;
using MongoDbCollectionMonitor.Commands.ExtractDocumentIdentifier;

namespace BannerflowDbCollectionMonitor.Commands.ProcessFeed
{
    public sealed class ProcessFeedEventRequest : ExtractDocumentIdentifierRequest
    {
        public override StudioCacheType CacheType => StudioCacheType.Feed;

        public override string PropertyNameToBeExtracted => "_id";
    }
}
