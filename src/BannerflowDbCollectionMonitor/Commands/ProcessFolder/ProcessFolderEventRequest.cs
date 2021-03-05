using MongoDbCollectionMonitor;
using MongoDbCollectionMonitor.Commands.ExtractDocumentIdentifier;

namespace BannerflowDbCollectionMonitor.Commands.ProcessFolder
{
    public class ProcessFolderEventRequest : ExtractDocumentIdentifierRequest
    {
        public override StudioCacheType CacheType => StudioCacheType.Folder;

        public override string PropertyNameToBeExtracted => "brand";
    }
}
