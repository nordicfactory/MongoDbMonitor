using MongoDbCollectionMonitor;
using MongoDbCollectionMonitor.Commands.ExtractDocumentIdentifier;

namespace AccountManagerDbCollectionMonitor.Commands.ProcessAccountInfo
{
    public sealed class ProcessAccountInfoEventRequest : ExtractDocumentIdentifierRequest
    {
        public override StudioCacheType CacheType => StudioCacheType.Account;

        public override string PropertyNameToBeExtracted => "accountId";
    }
}
