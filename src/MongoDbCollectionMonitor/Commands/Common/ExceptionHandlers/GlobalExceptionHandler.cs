using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace MongoDbCollectionMonitor.Commands.Common.ExceptionHandlers
{
    internal class GlobalExceptionHandler<TRequest, TResponse, TException> :
        IRequestExceptionHandler<TRequest, TResponse, TException>
        where TException : Exception
        where TResponse : new()
    {
        private readonly ILogger _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler<TRequest, TResponse, TException>> logger)
        {
            _logger = logger;
        }

        public Task Handle(
            TRequest request,
            TException exception,
            RequestExceptionHandlerState<TResponse> state,
            CancellationToken cancellationToken)
        {
            _logger.LogCritical(exception, exception.Message);

            state.SetHandled(new TResponse());

            return Task.CompletedTask;
        }
    }
}
