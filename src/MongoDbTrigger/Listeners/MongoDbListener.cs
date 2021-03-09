using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDbTrigger.DataAccess;

namespace MongoDbTrigger.Listeners
{
    internal class MongoDbListener : IListener
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private readonly ITriggeredFunctionExecutor _executor;
        private readonly MongoDbCollectionFactory _collectionFactory;

        private bool _disposedValue;

        public MongoDbListener(MongoDbCollectionFactory collectionFactory, ITriggeredFunctionExecutor executor)
        {
            _collectionFactory = collectionFactory;
            _executor = executor;
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        public Task StartAsync(CancellationToken cancellationToken) => WatchAsync(_cancellationTokenSource.Token);

        private async Task WatchAsync(CancellationToken cancellationToken)
        {
            var childCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            var collections = _collectionFactory.GetMongoCollection();
            var tasks = new List<Task>(collections.Count());

            foreach (var collection in collections)
            {
                var task = Watch(collection, childCancellation.Token);

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        private async Task Watch(IMongoCollection<dynamic> collection, CancellationToken cancellation)
        {
            var cursor = await collection.WatchAsync(null, cancellation);
            await cursor.ForEachAsync(document => WatchChange(document, cancellation), cancellation);
        }

        private async Task WatchChange(BsonDocumentBackedClass document, CancellationToken cancellation)
        {
            var input = new TriggeredFunctionData
            {
                TriggerValue = document
            };

            await _executor.TryExecuteAsync(input, cancellation);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
