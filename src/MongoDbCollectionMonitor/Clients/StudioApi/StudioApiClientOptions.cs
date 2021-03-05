using System;

namespace MongoDbCollectionMonitor.Clients.StudioApi
{
    public class StudioApiClientOptions
    {
        public Uri ClearCacheWebhook { get; set; }

        public int TimeoutInSeconds { get; set; }
    }
}
