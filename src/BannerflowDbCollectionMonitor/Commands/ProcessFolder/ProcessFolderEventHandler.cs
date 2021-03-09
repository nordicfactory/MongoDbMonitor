using MediatR;
using MongoDbCollectionMonitor.Commands.ExtractDocumentIdentifier;

namespace BannerflowDbCollectionMonitor.Commands.ProcessFolder
{
    public class ProcessFolderEventHandler : ExtractDocumentIdentifierHandler<ProcessFolderEventRequest>
    {
        public ProcessFolderEventHandler(IMediator mediator) : base(mediator)
        {
        }
    }
}
