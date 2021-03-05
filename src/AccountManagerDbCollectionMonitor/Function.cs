using Microsoft.Azure.WebJobs;
using MongoDB.Driver;
using MongoDbCollectionMonitor;
using MongoDbTrigger.Triggers;
using System.Threading;
using System.Threading.Tasks;

namespace AccountManagerDbCollectionMonitor
{
    // https://github.com/Azure/azure-functions-core-tools/issues/2294 - blocked upgrade to .net 5
    public class Function
    {
        private readonly MonitorRunner _runner;

        public Function(MonitorRunner runner)
        {
            _runner = runner;
        }

        [FunctionName("AccountManagerDbCollectionMonitor")]
        public async Task Run(
            [MongoDbTrigger(
                "accounts_sandbox",
                new[] {CollectionNames.Account, CollectionNames.AccountInfo},
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
