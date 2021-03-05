namespace MongoDbCollectionMonitor.Clients.SlackApi
{
    public class SlackApiClientOptions
    {
        public string ChannelWebhookUrl { get; set; }

        public int TimeoutInSeconds { get; set; }

    }
}