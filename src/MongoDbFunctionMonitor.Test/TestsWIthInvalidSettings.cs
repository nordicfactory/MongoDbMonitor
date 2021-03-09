using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDbCollectionMonitor;
using MongoDbCollectionMonitor.Commands.Common.Responses;
using Xunit;

namespace MongoDbMonitor.Test
{
    public class TestsWIthInvalidSettings
    {
        private static readonly Lazy<IServiceCollection> Services = new Lazy<IServiceCollection>(() => TestServiceFactory.RegisterServices(true, true, "test-with-no-collections.json"), true);

        [Fact]
        public async Task Should_Return_Unknown_ProcessingStep_When_Cancelled()
        {
            await using var provider = Services.Value.BuildServiceProvider(true);

            var runner = provider.GetRequiredService<MonitorRunner>();

            var run = runner.Run(
                "BF_Brand",
                "update",
                new Dictionary<string, object> { ["_id"] = ObjectId.GenerateNewId() },
                CancellationToken.None);

            var response = await run;

            Assert.False(response.IsSuccessfull);
            Assert.Equal(ProcessingStep.Unknown, response.FinalStep);
        }
    }
}
