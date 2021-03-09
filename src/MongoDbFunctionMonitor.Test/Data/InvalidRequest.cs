using MediatR;
using MongoDbCollectionMonitor.Commands.Common.Responses;

namespace MongoDbMonitor.Test.Data
{
    internal class InvalidRequest : IRequest<ProcessingStatusResponse>
    {
        public int Id { get; set; }
    }
}
