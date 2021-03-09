using MongoDbCollectionMonitor;
using MongoDbCollectionMonitor.Commands.ExtractDocumentIdentifier;

namespace BannerflowDbCollectionMonitor.Commands.ProcessBrand
{
    public sealed class ProcessBrandEventRequest : ExtractDocumentIdentifierRequest
    {
        public override StudioCacheType CacheType => StudioCacheType.Brand;

        public override string PropertyNameToBeExtracted => "_id";
    }
}
