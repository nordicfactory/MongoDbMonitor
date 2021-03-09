using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDbCollectionMonitor;
using MongoDbCollectionMonitor.Commands.Common.Responses;
using MongoDbCollectionMonitor.Commands.ExtractDocumentIdentifier;
using MongoDbCollectionMonitor.Commands.NotifyStudio;
using MongoDbCollectionMonitor.Commands.ProcessMongoEvent;
using MongoDbCollectionMonitor.Commands.ResolveCollectionType;
using MongoDbMonitor.Test.Data.MonitorRunner;
using Xunit;

namespace MongoDbMonitor.Test
{
    public class PipelineTestsWithMockedHttpClients
    {
        private static readonly Lazy<IServiceCollection> Services = new Lazy<IServiceCollection>(() => TestServiceFactory.RegisterServices(true, true), true);

        [Theory]
        [ClassData(typeof(GetCollectionAndRequestDataClass))]
        public async Task Shoud_Return_NotifyStudio_ProcessingStep(string collectionName, ExtractDocumentIdentifierRequest request)
        {
            await using var provider = Services.Value.BuildServiceProvider();

            var runner = provider.GetRequiredService<MonitorRunner>();

            var response = await runner.Run(collectionName,
                "update",
                new Dictionary<string, object>
                {
                    [request.PropertyNameToBeExtracted] = ObjectId.GenerateNewId(),
                    ["name"] = "My brand",
                    ["accountId"] = ObjectId.GenerateNewId()
                },
                CancellationToken.None);

            Assert.True(response.IsSuccessfull);
            Assert.Equal(ProcessingStep.NotifyStudio, response.FinalStep);
            Assert.Equal(4, response.Perf.Count);
            Assert.True(response.Perf.TryGetValue(nameof(ProcessMongoEventRequest), out _));
            Assert.True(response.Perf.TryGetValue(nameof(ResolveCollectionTypeRequest), out _));
            Assert.True(response.Perf.TryGetValue(request.GetType().Name, out _));
            Assert.True(response.Perf.TryGetValue(nameof(NotifyStudioRequest), out _));
        }
    }
}
