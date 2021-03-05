using MediatR;
using MongoDbCollectionMonitor.Commands.ExtractDocumentIdentifier;

namespace BannerflowDbCollectionMonitor.Commands.ProcessLocalization
{
    public class ProcessLocalizationEventHandler : ExtractDocumentIdentifierHandler<ProcessLocalizationEventRequest>
    {
        public ProcessLocalizationEventHandler(IMediator mediator) : base(mediator)
        {
        }
    }
}