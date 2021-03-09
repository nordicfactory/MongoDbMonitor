using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDbCollectionMonitor;
using MongoDbCollectionMonitor.Commands.Common.Responses;
using MongoDbCollectionMonitor.Commands.ExtractDocumentIdentifier;
using MongoDbMonitor.Test.Data.MonitorRunner;
using Xunit;

namespace MongoDbMonitor.Test
{
    public class PipelineTestsWithExceptionHandlers
    {
        private static readonly Lazy<IServiceCollection> Services = new Lazy<IServiceCollection>(() => TestServiceFactory.RegisterServices(true), true);

        [Theory]
        [ClassData(typeof(GetCollectionAndRequestDataClass))]
        public async Task Should_Return_SlackAlertSend_ProcessingStep(string collectionName, ExtractDocumentIdentifierRequest request)
        {
            await using var provider = Services.Value.BuildServiceProvider(true);

            var runner = provider.GetRequiredService<MonitorRunner>();

            var response = await runner.Run(
                collectionName,
                "update",
                new Dictionary<string, object>
                {
                    [request.PropertyNameToBeExtracted] = ObjectId.GenerateNewId(),
                    ["name"] = "My brand",
                    ["accountId"] = ObjectId.GenerateNewId()
                },
                CancellationToken.None);

            Assert.False(response.IsSuccessfull);
            Assert.Equal(ProcessingStep.SendSlackAlert, response.FinalStep);
        }

        [Fact]
        public async Task Should_Return_ResolveCollectionType_ProcessingStep_On_InvalidRequestTypeException()
        {
            await using var provider = Services.Value.BuildServiceProvider(true);

            var runner = provider.GetRequiredService<MonitorRunner>();

            var response = await runner.Run(
                "Test",
                "update",
                new Dictionary<string, object>
                {
                    ["_id"] = ObjectId.GenerateNewId()
                },
                CancellationToken.None);

            Assert.False(response.IsSuccessfull);
            Assert.Equal(ProcessingStep.ResolveCollectionType, response.FinalStep);
        }

        [Fact]
        public async Task Should_Return_ResolveCollectionType_ProcessingStep_On_MissingRequiredPropertyException()
        {
            await using var provider = Services.Value.BuildServiceProvider(true);

            var runner = provider.GetRequiredService<MonitorRunner>();

            var response = await runner.Run(
                "Test2",
                "update",
                new Dictionary<string, object> { ["_id"] = ObjectId.GenerateNewId() },
                CancellationToken.None);

            Assert.False(response.IsSuccessfull);
            Assert.Equal(ProcessingStep.ResolveCollectionType, response.FinalStep);
        }

        [Fact]
        public async Task Should_Return_ExtractDocumentIdentifier_ProcessingStep_On_PropertyNotFoundInDocumentException()
        {
            await using var provider = Services.Value.BuildServiceProvider(true);

            var runner = provider.GetRequiredService<MonitorRunner>();

            var response = await runner.Run(
                "BF_Brand",
                "update",
                new Dictionary<string, object> { ["nay_rolls"] = ObjectId.GenerateNewId() },
                CancellationToken.None);

            Assert.False(response.IsSuccessfull);
            Assert.Equal(ProcessingStep.ExtractDocumentIdentifier, response.FinalStep);
        }

        [Fact]
        public async Task Should_Return_ExtractDocumentIdentifier_ProcessingStep_On_InvalidObjectIdException()
        {
            await using var provider = Services.Value.BuildServiceProvider(true);

            var runner = provider.GetRequiredService<MonitorRunner>();

            var response = await runner.Run(
                "BF_Brand",
                "update",
                new Dictionary<string, object> { ["_id"] = 1 },
                CancellationToken.None);

            Assert.False(response.IsSuccessfull);
            Assert.Equal(ProcessingStep.ExtractDocumentIdentifier, response.FinalStep);
        }

        [Fact]
        public async Task Should_Return_ProcessMongoEvent_ProcessingStep_For_Not_Configured_Operation()
        {
            await using var provider = Services.Value.BuildServiceProvider(true);

            var runner = provider.GetRequiredService<MonitorRunner>();

            var response = await runner.Run(
                "BF_Brand",
                "drop",
                new Dictionary<string, object> { ["_id"] = 1 },
                CancellationToken.None);

            Assert.False(response.IsSuccessfull);
            Assert.Equal(ProcessingStep.ProcessMongoEvent, response.FinalStep);
        }
    }
}
