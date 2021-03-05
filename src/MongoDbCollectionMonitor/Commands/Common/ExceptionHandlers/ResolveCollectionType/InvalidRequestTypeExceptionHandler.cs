using MediatR;
using Microsoft.Extensions.Logging;
using MongoDbCollectionMonitor.Commands.Common.Exceptions;

namespace MongoDbCollectionMonitor.Commands.Common.ExceptionHandlers.ResolveCollectionType
{
    internal class InvalidRequestTypeExceptionHandler :
        ResolveCollectionTypeRequestExceptionHandler<InvalidRequestTypeException>
    {
        public InvalidRequestTypeExceptionHandler(
            IMediator mediator,
            ILogger<InvalidRequestTypeExceptionHandler> logger) :
            base(mediator, logger)
        {
        }
    }
}
