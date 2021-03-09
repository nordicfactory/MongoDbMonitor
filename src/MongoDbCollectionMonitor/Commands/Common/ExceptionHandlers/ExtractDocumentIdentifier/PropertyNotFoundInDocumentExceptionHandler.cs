using MediatR;
using Microsoft.Extensions.Logging;
using MongoDbCollectionMonitor.Commands.Common.Exceptions;
using MongoDbCollectionMonitor.Commands.ExtractDocumentIdentifier;

namespace MongoDbCollectionMonitor.Commands.Common.ExceptionHandlers.ExtractDocumentIdentifier
{
    internal class PropertyNotFoundInDocumentExceptionHandler<TRequest> :
        ExtractDocumentIdentifierRequestExceptionHandler<TRequest, PropertyNotFoundInDocumentException>
        where TRequest : ExtractDocumentIdentifierRequest
    {
        public PropertyNotFoundInDocumentExceptionHandler(
            IMediator mediator,
            ILogger<PropertyNotFoundInDocumentExceptionHandler<TRequest>> logger) :
            base(mediator, logger)
        {
        }
    }
}
