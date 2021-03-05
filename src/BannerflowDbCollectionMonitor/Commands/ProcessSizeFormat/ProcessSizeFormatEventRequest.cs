using MongoDbCollectionMonitor;
using MongoDbCollectionMonitor.Commands.ExtractDocumentIdentifier;

namespace BannerflowDbCollectionMonitor.Commands.ProcessSizeFormat
{
    public sealed class ProcessSizeFormatEventRequest : ExtractDocumentIdentifierRequest
    {
        public override StudioCacheType CacheType => StudioCacheType.BrandSettings;

        public override string PropertyNameToBeExtracted => "brandId";
    }
}
