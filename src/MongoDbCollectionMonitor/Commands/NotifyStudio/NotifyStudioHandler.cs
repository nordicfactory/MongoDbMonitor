using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MongoDbCollectionMonitor.Clients.StudioApi;
using MongoDbCollectionMonitor.Commands.Common.Exceptions;
using MongoDbCollectionMonitor.Commands.Common.Responses;

namespace MongoDbCollectionMonitor.Commands.NotifyStudio
{
    internal class NotifyStudioHandler : IRequestHandler<NotifyStudioRequest, ProcessingStatusResponse>
    {
        private readonly IStudioApiClient _client;

        public NotifyStudioHandler(IStudioApiClient client)
        {
            _client = client;
        }

        public async Task<ProcessingStatusResponse> Handle(NotifyStudioRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await _client.NotifyStudio(request.CacheType, request.Id, cancellationToken);
                
                return new ProcessingStatusResponse {FinalStep = ProcessingStep.NotifyStudio};
            }
            catch (Exception e)
            {
                throw new NotifyStudioFailedException(e);
            }
        }
    }
}
