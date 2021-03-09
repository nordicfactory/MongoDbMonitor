using BannerflowDbCollectionMonitor.Commands.ProcessBrand;
using BannerflowDbCollectionMonitor.Commands.ProcessFeed;
using BannerflowDbCollectionMonitor.Commands.ProcessFolder;
using BannerflowDbCollectionMonitor.Commands.ProcessLocalization;
using BannerflowDbCollectionMonitor.Commands.ProcessSizeFormat;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using MongoDbCollectionMonitor;

[assembly: FunctionsStartup(typeof(BannerflowDbCollectionMonitor.Startup))]
namespace BannerflowDbCollectionMonitor
{
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.RegisterProcessDocumentMediatorHandler<ProcessBrandEventRequest, ProcessBrandEventHandler>();
            builder.RegisterProcessDocumentMediatorHandler<ProcessFolderEventRequest, ProcessFolderEventHandler>();
            builder.RegisterProcessDocumentMediatorHandler<ProcessFeedEventRequest, ProcessFeedEventHandler>();
            builder.RegisterProcessDocumentMediatorHandler<ProcessLocalizationEventRequest, ProcessLocalizationEventHandler>();
            builder.RegisterProcessDocumentMediatorHandler<ProcessSizeFormatEventRequest, ProcessSizeFormatEventHandler>();

            builder.AddMongoDbCollectionMonitor();
        }
    }
}
