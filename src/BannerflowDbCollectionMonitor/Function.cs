using Microsoft.Azure.WebJobs;
using MongoDB.Driver;
using MongoDbCollectionMonitor;
using MongoDbTrigger.Triggers;
using System.Threading;
using System.Threading.Tasks;

namespace BannerflowDbCollectionMonitor
{
    // https://github.com/Azure/azure-functions-core-tools/issues/2294 - blocked upgrade to .net 5
    public class Function
    {
        private readonly MonitorRunner _runner;

        public Function(MonitorRunner runner)
        {
            _runner = runner;
        }

        [FunctionName("BannerflowDbCollectionMonitor")]
        public async Task Run(
            [MongoDbTrigger(
                "bannerflow_sandbox",
                new[] { CollectionNames.Feeds, CollectionNames.Brands, CollectionNames.Folders, CollectionNames.Localizations, CollectionNames.SizeFormats },
                ConnectionString = "%Connection")]
            ChangeStreamDocument<dynamic> document,
            CancellationToken cancellation)
        {
            await _runner.Run(
                document.CollectionNamespace.CollectionName,
                document.OperationType.ToString(),
                document.FullDocument,
                cancellation);
        }
    }
}
