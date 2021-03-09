using System;

namespace MongoDbCollectionMonitor.Clients.SlackApi
{
    internal class SlackApiClientOptions
    {
        public Uri ChannelWebhookUrl { get; set; }

        public int TimeoutInSeconds { get; set; }

    }
}