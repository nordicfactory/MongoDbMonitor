using System;
using System.Linq;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDbCollectionMonitor;
using MongoDbCollectionMonitor.Clients.SlackApi;
using MongoDbCollectionMonitor.Clients.StudioApi;
using MongoDbCollectionMonitor.CrossCutting.QoS;
using MongoDbTrigger.DataAccess;
using MongoDbTrigger.Extensions;
using Xunit;

namespace MongoDbMonitor.Test
{
    public class DependencyInjectionTests
    {
        private static readonly Lazy<IServiceCollection> TriggerServices = new Lazy<IServiceCollection>(() => TestServiceFactory.RegisterMongoDbTrigger("test-di.json"), true);
        private static readonly Lazy<IServiceCollection> MonitorServices = new Lazy<IServiceCollection>(() => TestServiceFactory.RegisterServices(true, true, "test-di.json"), true);

        [Fact]
        public void Should_Resolve_MongoCollectionFactory_With_Options()
        {
            var provider = TriggerServices.Value.BuildServiceProvider();

            var factory = provider.GetRequiredService<MongoDbCollectionFactory>();

            Assert.Equal("test-db", factory.Options.Database);
            Assert.Equal("mongodb://mongodb:27017", factory.Options.ConnectionString);
            Assert.Equal(2, factory.Options.Collections.Count);

            var firstName = factory.Options.Collections.First();
            var secondName = factory.Options.Collections.Last();

            Assert.Equal("Test", firstName);
            Assert.Equal("Test2", secondName);
        }

        [Fact]
        public void Should_Resolve_StudioApiClient_With_Options()
        {
            using var provider = MonitorServices.Value.BuildServiceProvider();

            var client = (StudioApiClient)provider.GetService<IStudioApiClient>();

            Assert.Equal(new Uri("http://studio.local/api/webhook/incoming/cache/clear"), client.Options.ClearCacheWebhook);
            Assert.Equal(5, client.Options.TimeoutInSeconds);
        }

        [Fact]
        public void Should_Resolve_SlackApiClient_With_Options()
        {
            using var provider = MonitorServices.Value.BuildServiceProvider();

            var client = (SlackApiClient) provider.GetService<ISlackApiClient>();

            Assert.Equal(new Uri("https://hooks.slack.com/services/xxx"), client.Options.ChannelWebhookUrl);
            Assert.Equal(5, client.Options.TimeoutInSeconds);
        }

        [Fact]
        public void Should_Resolve_RetryProvider_With_Options()
        {
            using var provider = MonitorServices.Value.BuildServiceProvider();

            var retrier = (RetryProvider)provider.GetRequiredService<IRetryProvider>();

            Assert.Equal(100, retrier.Options.JitterMaximum);
            Assert.Equal(3, retrier.Options.Delays.Count);
            Assert.Equal(50, retrier.Options.Delays[0]);
            Assert.Equal(100, retrier.Options.Delays[1]);
            Assert.Equal(200, retrier.Options.Delays[2]);
        }

        [Fact]
        public void Should_Initialize_MongoDbTriggerExtensionsConfigProvider()
        {
            using var provider = TriggerServices.Value.BuildServiceProvider();

            var factory = provider.GetRequiredService<MongoDbCollectionFactory>();

            var configProvider = new MongoDbTriggerExtensionsConfigProvider(factory);

            var context = new ExtensionConfigContext(
                provider.GetService<IConfiguration>(),
                null,
                null,
                null,
                new DefaultExtensionRegistry());

            configProvider.Initialize(context);
        }
    }
}
