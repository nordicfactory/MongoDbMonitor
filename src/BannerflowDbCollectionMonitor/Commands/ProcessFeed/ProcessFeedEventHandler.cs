using MediatR;
using MongoDbCollectionMonitor.Commands.ExtractDocumentIdentifier;

namespace BannerflowDbCollectionMonitor.Commands.ProcessFeed
{
    public class ProcessFeedEventHandler : ExtractDocumentIdentifierHandler<ProcessFeedEventRequest>
    {
        public ProcessFeedEventHandler(IMediator mediator) : base(mediator)
        {
        }
    }
}
