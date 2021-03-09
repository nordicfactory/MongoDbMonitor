using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDbCollectionMonitor.Commands.Common.Responses;
using MongoDbCollectionMonitor.CrossCutting.Performance;

namespace MongoDbCollectionMonitor.Commands.Common.Behaviors
{
    internal class MetricsCapturingPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TResponse : ProcessingStatusResponse
    {
        private readonly ILogger _logger;

        public MetricsCapturingPipelineBehavior(ILogger<MetricsCapturingPipelineBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var requestType = request.GetType().Name;

            _logger.LogDebug("{RequestType} - started.", requestType);

            var stopwatch = ValueStopwatch.StartNew();

            TResponse response = await next();

            response.Perf[typeof(TRequest).Name] = stopwatch.GetElapsedTime().Milliseconds;

            _logger.LogDebug("{RequestType} - finished. {Elapsed}", requestType, stopwatch.GetElapsedTime().Milliseconds.ToString());

            return response;
        }
    }
}
