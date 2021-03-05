using MediatR;
using MongoDB.Driver;
using MongoDbCollectionMonitor.Commands.Common.Responses;
using MongoDbCollectionMonitor.Commands.ProcessMongoEvent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDbCollectionMonitor
{
    public sealed class MonitorRunner
    {
        private static readonly IReadOnlyCollection<string> AllowedOperations = new List<string>
        {
            nameof(ChangeStreamOperationType.Insert).ToLowerInvariant(),
            nameof(ChangeStreamOperationType.Update).ToLowerInvariant(),
            nameof(ChangeStreamOperationType.Replace).ToLowerInvariant(),
            nameof(ChangeStreamOperationType.Delete).ToLowerInvariant(),
            nameof(ChangeStreamOperationType.Invalidate).ToLowerInvariant(),
            nameof(ChangeStreamOperationType.Rename).ToLowerInvariant(),
            nameof(ChangeStreamOperationType.Drop).ToLowerInvariant()
        };

        private readonly IMediator _mediator;

        public MonitorRunner(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<ProcessingStatusResponse> Run(string collectionName, string operationName, IDictionary<string, object> values, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(collectionName))
                throw new ArgumentNullException(nameof(collectionName));

            var name = string.IsNullOrWhiteSpace(operationName) ? string.Empty : operationName.Trim().ToLowerInvariant();

            if (AllowedOperations.All(x => x != name))
                throw new ArgumentOutOfRangeException($"OperationName: {operationName} is not allowed.");

            if (values == null || values.Count == 0)
                throw new ArgumentNullException(nameof(values));

            var childCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellation);

            var response = await
                _mediator.Send(
                    new ProcessMongoEventRequest
                    {
                        CollectionName = collectionName,
                        OperationName = operationName,
                        Values = values
                    },
                    childCancellation.Token);

            return response;
        }
    }
}
