using AccountManagerDbCollectionMonitor;
using AccountManagerDbCollectionMonitor.Commands.ProcessAccount;
using AccountManagerDbCollectionMonitor.Commands.ProcessAccountInfo;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using MongoDbCollectionMonitor;

[assembly: FunctionsStartup(typeof(Startup))]
namespace AccountManagerDbCollectionMonitor
{
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.RegisterProcessDocumentMediatorHandler<ProcessAccountEventRequest, ProcessAccountEventHandler>();
            builder.RegisterProcessDocumentMediatorHandler<ProcessAccountInfoEventRequest, ProcessAccountInfoEventHandler>();

            builder.AddMongoDbCollectionMonitor();
        }
    }
}
