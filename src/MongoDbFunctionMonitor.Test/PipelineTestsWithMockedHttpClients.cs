using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AccountManagerDbCollectionMonitor.Commands.ProcessAccount;
using AccountManagerDbCollectionMonitor.Commands.ProcessAccountInfo;
using BannerflowDbCollectionMonitor.Commands.ProcessBrand;
using BannerflowDbCollectionMonitor.Commands.ProcessFeed;
using BannerflowDbCollectionMonitor.Commands.ProcessFolder;
using BannerflowDbCollectionMonitor.Commands.ProcessLocalization;
using BannerflowDbCollectionMonitor.Commands.ProcessSizeFormat;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDbCollectionMonitor;
using MongoDbCollectionMonitor.Commands.Common.Responses;
using MongoDbCollectionMonitor.Commands.NotifyStudio;
using MongoDbCollectionMonitor.Commands.ProcessMongoEvent;
using MongoDbCollectionMonitor.Commands.ResolveCollectionType;
using Xunit;

namespace MongoDbMonitor.Test
{
    public class PipelineTestsWithMockedHttpClients
    {
        private static readonly Lazy<IServiceCollection> Services = new Lazy<IServiceCollection>(() => TestServiceFactory.RegisterServices(true, true), true);

        [Theory]
        [InlineData("DataAccount", "_id", nameof(ProcessAccountEventRequest))]
        [InlineData("DataAccountInfo", "accountId", nameof(ProcessAccountInfoEventRequest))]
        [InlineData("BF_Brand", "_id", nameof(ProcessBrandEventRequest))]
        [InlineData("BF_Localization", "brandId", nameof(ProcessLocalizationEventRequest))]
        [InlineData("BF_SizeFormat", "brandId", nameof(ProcessSizeFormatEventRequest))]
        [InlineData("BF_Folder", "_id", nameof(ProcessFolderEventRequest))]
        [InlineData("BF_Feed", "_id", nameof(ProcessFeedEventRequest))]
        public async Task Shoud_Return_NotifyStudio_ProcessingStep(string collectionName, string requiredProperty, string requestName)
        {
            await using var provider = Services.Value.BuildServiceProvider();

            var runner = provider.GetRequiredService<MonitorRunner>();

            var response = await runner.Run(collectionName,
                "update",
                new Dictionary<string, object>
                {
                    [requiredProperty] = ObjectId.GenerateNewId(),
                    ["name"] = "My brand",
                    ["accountId"] = ObjectId.GenerateNewId()
                },
                CancellationToken.None);

            Assert.True(response.IsSuccessfull);
            Assert.Equal(ProcessingStep.NotifyStudio, response.FinalStep);
            Assert.Equal(4, response.Perf.Count);
            Assert.True(response.Perf.TryGetValue(nameof(ProcessMongoEventRequest), out _));
            Assert.True(response.Perf.TryGetValue(nameof(ResolveCollectionTypeRequest), out _));
            Assert.True(response.Perf.TryGetValue(requestName, out _));
            Assert.True(response.Perf.TryGetValue(nameof(NotifyStudioRequest), out _));
        }
    }
}
