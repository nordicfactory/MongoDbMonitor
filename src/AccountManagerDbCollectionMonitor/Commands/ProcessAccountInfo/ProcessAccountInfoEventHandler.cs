using MediatR;
using MongoDbCollectionMonitor.Commands.ExtractDocumentIdentifier;

namespace AccountManagerDbCollectionMonitor.Commands.ProcessAccountInfo
{
    public class ProcessAccountInfoEventHandler : ExtractDocumentIdentifierHandler<ProcessAccountInfoEventRequest>
    {
        public ProcessAccountInfoEventHandler(IMediator mediator) : base(mediator)
        {
        }

    }
}
