using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using MongoDbCollectionMonitor.Commands.Common.Responses;
using MongoDbCollectionMonitor.Commands.ExtractDocumentIdentifier;
using MongoDbCollectionMonitor.Commands.SendSlackAlert;

namespace MongoDbCollectionMonitor.Commands.Common.ExceptionHandlers.ExtractDocumentIdentifier
{
    internal abstract class ExtractDocumentIdentifierRequestExceptionHandler<TRequest, TException> :
        IRequestExceptionHandler<TRequest, ProcessingStatusResponse, TException>
        where TRequest : ExtractDocumentIdentifierRequest
        where TException : Exception
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        protected ExtractDocumentIdentifierRequestExceptionHandler(
            IMediator mediator,
            ILogger<ExtractDocumentIdentifierRequestExceptionHandler<TRequest, TException>> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Handle(
            TRequest request,
            TException exception,
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
