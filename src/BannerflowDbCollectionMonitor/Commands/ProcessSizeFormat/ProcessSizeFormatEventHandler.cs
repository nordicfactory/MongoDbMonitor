using MediatR;
using MongoDbCollectionMonitor.Commands.ExtractDocumentIdentifier;

namespace BannerflowDbCollectionMonitor.Commands.ProcessSizeFormat
{
    public class ProcessSizeFormatEventHandler : ExtractDocumentIdentifierHandler<ProcessSizeFormatEventRequest>
    {
        public ProcessSizeFormatEventHandler(IMediator mediator) : base(mediator)
        {
        }
    }
}
