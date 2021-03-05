using MediatR;
using MongoDbCollectionMonitor.Commands.ExtractDocumentIdentifier;

namespace BannerflowDbCollectionMonitor.Commands.ProcessBrand
{
    public sealed class ProcessBrandEventHandler : ExtractDocumentIdentifierHandler<ProcessBrandEventRequest>
    {
        public ProcessBrandEventHandler(IMediator mediator) : base(mediator)
        {
        }
    }
}
