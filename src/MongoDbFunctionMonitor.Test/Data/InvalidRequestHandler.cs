using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MongoDbCollectionMonitor.Commands.Common.Responses;

namespace MongoDbMonitor.Test.Data
{
    internal class InvalidRequestHandler : IRequestHandler<InvalidRequest, ProcessingStatusResponse>
    {
        public Task<ProcessingStatusResponse> Handle(InvalidRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new ProcessingStatusResponse());
        }
    }
}
