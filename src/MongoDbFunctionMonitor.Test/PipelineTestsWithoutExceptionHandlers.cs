using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDbCollectionMonitor;
using MongoDbCollectionMonitor.Commands.Common.Exceptions;
using Xunit;

namespace MongoDbMonitor.Test
{
    public class PipelineTestsWithoutExceptionHandlers
    {
        private static readonly Lazy<IServiceCollection> Services = new Lazy<IServiceCollection>(() => TestServiceFactory.RegisterServices(), true);

        [Theory]
        [InlineData("DataAccount", "_id")]
        [InlineData("DataAccountInfo", "accountId")]
        [InlineData("BF_Brand", "_id")]
        [InlineData("BF_Localization", "brandId")]
        [InlineData("BF_SizeFormat", "brandId")]
        [InlineData("BF_Folder", "brand")]
        [InlineData("BF_Feed", "_id")]
        public async Task Should_Throw_NotifyStudioFailedException(string collectionName, string requiredProperty)
        {
            await using var provider = Services.Value.BuildServiceProvider();

            var runner = provider.GetRequiredService<MonitorRunner>();

            await
                Assert.ThrowsAsync<NotifyStudioFailedException>(
                    () =>
                        runner.Run(
                            collectionName,
                            "update",
                            new Dictionary<string, object>
                            {
                                [requiredProperty] = ObjectId.GenerateNewId(),
                                ["name"] = "My brand",
                                ["accountId"] = ObjectId.GenerateNewId()
                            },
                            CancellationToken.None));
        }

        [Fact]
        public async Task Shoud_Throw_InvalidRequestTypeException()
        {
            await using var provider = Services.Value.BuildServiceProvider();

            var runner = provider.GetRequiredService<MonitorRunner>();

            await
                Assert.ThrowsAsync<InvalidRequestTypeException>(
                    () =>
                        runner.Run(
                            "Test",
                            "update",
                            new Dictionary<string, object>
                            {
                                ["_id"] = ObjectId.GenerateNewId()
                            },
                            CancellationToken.None));
        }

        [Fact]
        public async Task Should_Throw_MissingRequiredPropertyException()
        {
            await using var provider = Services.Value.BuildServiceProvider();

            var runner = provider.GetRequiredService<MonitorRunner>();

            await
                Assert.ThrowsAsync<MissingRequiredPropertyException>(
                    () =>
                        runner.Run(
                            "Test2",
                            "update",
                            new Dictionary<string, object>
                            {
                                ["_id"] = ObjectId.GenerateNewId()
                            },
                            CancellationToken.None));
        }

        [Fact]
        public async Task Should_Throw_PropertyNotFoundInDocumentException()
        {
            await using var provider = Services.Value.BuildServiceProvider();

            var runner = provider.GetRequiredService<MonitorRunner>();

            await
                Assert.ThrowsAsync<PropertyNotFoundInDocumentException>(
                    () =>
                        runner.Run(
                            "BF_Brand",
                            "update",
                            new Dictionary<string, object>
                            {
                                ["nay_rolls"] = ObjectId.GenerateNewId()
                            },
                            CancellationToken.None));
        }

        [Fact]
        public async Task Should_Throw_InvalidObjectIdException()
        {
            await using var provider = Services.Value.BuildServiceProvider();

            var runner = provider.GetRequiredService<MonitorRunner>();

            await
                Assert.ThrowsAsync<InvalidObjectIdException>(
                    () =>
                        runner.Run(
                            "BF_Brand",
                            "update",
                            new Dictionary<string, object>
                            {
                                ["_id"] = 1
                            },
                            CancellationToken.None));
        }
    }
}
