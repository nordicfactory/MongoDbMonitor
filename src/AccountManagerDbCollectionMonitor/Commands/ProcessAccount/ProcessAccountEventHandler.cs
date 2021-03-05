using MediatR;
using MongoDbCollectionMonitor.Commands.ExtractDocumentIdentifier;

namespace AccountManagerDbCollectionMonitor.Commands.ProcessAccount
{
    public class ProcessAccountEventHandler : ExtractDocumentIdentifierHandler<ProcessAccountEventRequest>
    {
        public ProcessAccountEventHandler(IMediator mediator) : base(mediator)
        {
        }
    }
}
