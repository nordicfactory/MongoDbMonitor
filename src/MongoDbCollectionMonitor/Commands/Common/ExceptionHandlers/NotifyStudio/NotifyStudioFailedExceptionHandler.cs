using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using MongoDbCollectionMonitor.Commands.Common.Exceptions;
using MongoDbCollectionMonitor.Commands.Common.Responses;
using MongoDbCollectionMonitor.Commands.NotifyStudio;
using MongoDbCollectionMonitor.Commands.SendSlackAlert;

namespace MongoDbCollectionMonitor.Commands.Common.ExceptionHandlers.NotifyStudio
{
    internal class NotifyStudioFailedExceptionHandler :
        IRequestExceptionHandler<NotifyStudioRequest, ProcessingStatusResponse, NotifyStudioFailedException>
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public NotifyStudioFailedExceptionHandler(
            IMediator mediator,
            ILogger<NotifyStudioFailedExceptionHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Handle(
            NotifyStudioRequest request,
            NotifyStudioFailedException exception,
            RequestExceptionHandlerState<ProcessingStatusResponse> state,
            CancellationToken cancellationToken)
        {
            _logger.LogError(
                exception.InnerException,
                exception.Message);

            var response =
                await
                    _mediator.Send(new SendSlackAlertRequest
                        {
                            RequestType = request.GetType().FullName,
                            FailureReason = exception.Message,
                            RequestData = new Dictionary<string, object>
                            {
                                [nameof(request.CacheType)] = request.CacheType,
                                [nameof(request.Id)] = request.Id
                            }
                        },
                        cancellationToken);

            state.SetHandled(response);
        }
    }
}
