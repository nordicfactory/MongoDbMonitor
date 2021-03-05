using MediatR;
using MongoDB.Bson;
using MongoDbCollectionMonitor.Commands.Common.Responses;

namespace MongoDbCollectionMonitor.Commands.NotifyStudio
{
    public sealed class NotifyStudioRequest : IRequest<ProcessingStatusResponse>
    {
        public StudioCacheType CacheType { get; set; }

        public ObjectId Id { get; set; }
    }
}
