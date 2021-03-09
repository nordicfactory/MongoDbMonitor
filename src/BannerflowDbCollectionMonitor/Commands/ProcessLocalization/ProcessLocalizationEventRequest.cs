using MongoDbCollectionMonitor;
using MongoDbCollectionMonitor.Commands.ExtractDocumentIdentifier;

namespace BannerflowDbCollectionMonitor.Commands.ProcessLocalization
{
    public sealed class ProcessLocalizationEventRequest : ExtractDocumentIdentifierRequest
    {
        public override StudioCacheType CacheType => StudioCacheType.BrandSettings;

        public override string PropertyNameToBeExtracted => "brandId";
    }
}
