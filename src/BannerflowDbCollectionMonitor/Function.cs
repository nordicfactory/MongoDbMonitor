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
        private readonly CancellationTokenSource _source = new CancellationTokenSource();
        private readonly MonitorRunner _runner;

        public Function(MonitorRunner runner)
        {
            _runner = runner;
        }

        [FunctionName("BannerflowDbCollectionMonitor")]
        public async Task Run([MongoDbTrigger] ChangeStreamDocument<dynamic> document)
        {
            await _runner.Run(
                document.CollectionNamespace.CollectionName,
                document.OperationType.ToString(),
                document.FullDocument,
                _source.Token);
        }
    }
}
