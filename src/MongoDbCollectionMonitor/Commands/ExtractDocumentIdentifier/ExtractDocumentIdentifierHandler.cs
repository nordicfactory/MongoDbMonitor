using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MongoDB.Bson;
using MongoDbCollectionMonitor.Commands.Common.Exceptions;
using MongoDbCollectionMonitor.Commands.Common.Responses;
using MongoDbCollectionMonitor.Commands.NotifyStudio;

namespace MongoDbCollectionMonitor.Commands.ExtractDocumentIdentifier
{
    public abstract class ExtractDocumentIdentifierHandler<T> :
        IRequestHandler<T, ProcessingStatusResponse> where T : ExtractDocumentIdentifierRequest
    {
        private readonly IMediator _mediator;

        protected ExtractDocumentIdentifierHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<ProcessingStatusResponse> Handle(T request, CancellationToken cancellationToken)
        {
            if (!request.Values.TryGetValue(request.PropertyNameToBeExtracted, out var value))
                throw new PropertyNotFoundInDocumentException(request.PropertyNameToBeExtracted);

            if (!ObjectId.TryParse(value.ToString(), out var id))
                throw new InvalidObjectIdException(value.ToString());

            var response = await _mediator.Send(new NotifyStudioRequest
                {
                    CacheType = request.CacheType,
                    Id = id
                },
                cancellationToken);

            return response;
        }
    }
}
