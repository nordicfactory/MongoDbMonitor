using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using MongoDbCollectionMonitor.Commands.Common.Exceptions;
using MongoDbCollectionMonitor.Commands.Common.Responses;
using MongoDbCollectionMonitor.Commands.ExtractDocumentIdentifier;
using MongoDbCollectionMonitor.Commands.SendSlackAlert;

namespace MongoDbCollectionMonitor.Commands.Common.ExceptionHandlers.ExtractDocumentIdentifier
{
    internal class InvalidObjectIdExceptionHandler<TRequest> :
        IRequestExceptionHandler<TRequest, ProcessingStatusResponse, InvalidObjectIdException>
        where TRequest : ExtractDocumentIdentifierRequest
    {
        private readonly IMediator _mediator;
        private readonly ILogger<InvalidObjectIdExceptionHandler<TRequest>> _logger;

        public InvalidObjectIdExceptionHandler(
            IMediator mediator,
            ILogger<InvalidObjectIdExceptionHandler<TRequest>> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Handle(
            TRequest request,
            InvalidObjectIdException exception,
            RequestExceptionHandlerState<ProcessingStatusResponse> state,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, exception.Message);

            _ = await
                _mediator.Send(
                    new SendSlackAlertRequest
                    {
                        RequestType = request.GetType().FullName,
                        FailureReason = exception.Message,
                        RequestData = new Dictionary<string, object>
                        {
                            [nameof(request.CacheType)] = request.CacheType,
                            [nameof(request.PropertyNameToBeExtracted)] = request.PropertyNameToBeExtracted,
                            [nameof(request.Values)] = request.Values
                        }
                    },
                    cancellationToken);

            state.SetHandled(new ProcessingStatusResponse { FinalStep = ProcessingStep.ExtractDocumentIdentifier });
        }
    }
}