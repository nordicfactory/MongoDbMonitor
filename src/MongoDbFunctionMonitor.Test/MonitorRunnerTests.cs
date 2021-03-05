using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDbCollectionMonitor;
using MongoDbMonitor.Test.Data;
using Xunit;

namespace MongoDbMonitor.Test
{
    public class MonitorRunnerTests
    {
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task Should_Throw_ArgumentNullException_For_CollectionName(string collectionName)
        {
            await
                Assert.ThrowsAsync<ArgumentNullException>(() => new MonitorRunner(null).Run(
                    collectionName,
                    "insert",
                    new Dictionary<string, object> { ["_id"] = ObjectId.GenerateNewId() },
                    CancellationToken.None));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("poo")]
        public async Task Should_Throw_ArgumentOutOfRandeException_For_OpeartionName(string opertationName)
        {
            await
                Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => new MonitorRunner(null).Run(
                    "test",
                    opertationName,
                    new Dictionary<string, object> { ["_id"] = ObjectId.GenerateNewId() },
                    CancellationToken.None));
        }

        [Theory]
        [ClassData(typeof(ValuesDataClass))]
        public async Task Should_Throw_ArgumentNullException_For_Values(IDictionary<string, object> values)
        {
            await
                Assert.ThrowsAsync<ArgumentNullException>(() => new MonitorRunner(null).Run(
                    "test",
                    "insert",
                    values,
                    CancellationToken.None));
        }
    }
}
