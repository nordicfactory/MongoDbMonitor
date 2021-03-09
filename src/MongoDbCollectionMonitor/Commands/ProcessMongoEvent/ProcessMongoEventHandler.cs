using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using MongoDbCollectionMonitor.Commands.Common.Responses;
using MongoDbCollectionMonitor.Commands.ResolveCollectionType;

namespace MongoDbCollectionMonitor.Commands.ProcessMongoEvent
{
    public class ProcessMongoEventHandler : IRequestHandler<ProcessMongoEventRequest, ProcessingStatusResponse>
    {
        private readonly Collection<CollectionOptions> _options;
        private readonly IMediator _mediator;

        public ProcessMongoEventHandler(IOptions<Collection<CollectionOptions>> options, IMediator mediator)
        {
            _options = options.Value;
            _mediator = mediator;
        }

        public async Task<ProcessingStatusResponse> Handle(ProcessMongoEventRequest request, CancellationToken cancellationToken)
        {
            var collection =
                _options.First(
                    x =>
                        x.Name.Equals(
                            request.CollectionName,
                            StringComparison.InvariantCultureIgnoreCase));

            var operations = GetOperations(collection.OperationTypes);

            if (!operations.Any(x => x.Equals(request.OperationName, StringComparison.InvariantCultureIgnoreCase)))
                return new ProcessingStatusResponse{FinalStep = ProcessingStep.ProcessMongoEvent};

            var (assemblyName, requestName) = GetRequestName(_options, request.CollectionName);

            var response = await _mediator.Send(
                new ResolveCollectionTypeRequest
                {
                    AssemblyName = assemblyName,
                    HandlerRequestFullyQualifiedName = requestName,
                    Values = request.Values
                },
                cancellationToken);

            return response;
        }

        private static (string assemblyName, string requestName) GetRequestName(Collection<CollectionOptions> collections, string collectionName)
        {
            var collection =
                collections
                    .First(x => x.Name.Equals(collectionName, StringComparison.InvariantCultureIgnoreCase));

            return (collection.AssemblyName, collection.HandlerRequestFullQualifiedName);
        }

        private static IEnumerable<string> GetOperations(IEnumerable<string> operationNames)
        {
            return operationNames.Select(name => name.ToLowerInvariant());
        }
    }
}
