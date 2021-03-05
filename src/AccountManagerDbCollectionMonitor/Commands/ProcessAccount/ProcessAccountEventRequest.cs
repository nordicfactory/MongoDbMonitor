using MongoDbCollectionMonitor;
using MongoDbCollectionMonitor.Commands.ExtractDocumentIdentifier;

namespace AccountManagerDbCollectionMonitor.Commands.ProcessAccount
{
    public sealed class ProcessAccountEventRequest : ExtractDocumentIdentifierRequest
    {
        public override string PropertyNameToBeExtracted => "_id";

        public override StudioCacheType CacheType => StudioCacheType.Account;
    }
}
