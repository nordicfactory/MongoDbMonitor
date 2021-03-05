using MediatR;
using Microsoft.Extensions.Logging;
using MongoDbCollectionMonitor.Commands.Common.Exceptions;

namespace MongoDbCollectionMonitor.Commands.Common.ExceptionHandlers.ResolveCollectionType
{
    internal class MissingRequiredPropertyExceptionHandler :
        ResolveCollectionTypeRequestExceptionHandler<MissingRequiredPropertyException>
    {
        public MissingRequiredPropertyExceptionHandler(
            IMediator mediator,
            ILogger<MissingRequiredPropertyExceptionHandler> logger) :
            base(mediator, logger)
        {
        }
    }
}
