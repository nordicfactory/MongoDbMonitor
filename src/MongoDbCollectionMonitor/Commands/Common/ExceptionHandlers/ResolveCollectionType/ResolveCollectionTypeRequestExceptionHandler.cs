using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using MongoDbCollectionMonitor.Commands.Common.Responses;
using MongoDbCollectionMonitor.Commands.ResolveCollectionType;
using MongoDbCollectionMonitor.Commands.SendSlackAlert;

namespace MongoDbCollectionMonitor.Commands.Common.ExceptionHandlers.ResolveCollectionType
{
    internal abstract class ResolveCollectionTypeRequestExceptionHandler<TException> :
        IRequestExceptionHandler<ResolveCollectionTypeRequest, ProcessingStatusResponse, TException>
        where TException : Exception
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        protected ResolveCollectionTypeRequestExceptionHandler(
            IMediator mediator,
            ILogger<ResolveCollectionTypeRequestExceptionHandler<TException>> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Handle(
            ResolveCollectionTypeRequest request,
            TException exception,
            RequestExceptionHandlerState<ProcessingStatusResponse> state,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, exception.Message);

            _ =
                await
                _mediator.Send(
                    new SendSlackAlertRequest
                    {
                        RequestType = request.GetType().FullName,
                        FailureReason = exception.Message,
                        RequestData = new Dictionary<string, object>
                        {
                            [nameof(request.AssemblyName)] = request.AssemblyName,
                            [nameof(request.HandlerRequestFullyQualifiedName)] = request.HandlerRequestFullyQualifiedName,
                            [nameof(request.Values)] = request.Values
                        }
                    },
                    cancellationToken);

            state.SetHandled(new ProcessingStatusResponse { FinalStep = ProcessingStep.ResolveCollectionType });
        }
    }
}
