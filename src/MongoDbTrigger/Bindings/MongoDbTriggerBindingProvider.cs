using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Triggers;
using MongoDB.Driver;
using MongoDbTrigger.DataAccess;
using MongoDbTrigger.Triggers;

namespace MongoDbTrigger.Bindings
{
    internal sealed class MongoDbBindingProvider : ITriggerBindingProvider
    {
        private readonly MongoDbCollectionFactory _collectionFactory;

        public MongoDbBindingProvider(MongoDbCollectionFactory collectionFactory)
        {
            _collectionFactory = collectionFactory;
        }

        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context) => Task.FromResult(TryCreate(context));

        public ITriggerBinding TryCreate(TriggerBindingProviderContext context)
        {
            var parameter = context?.Parameter ?? throw new ArgumentNullException(nameof(context));

            if (parameter.ParameterType.GetGenericTypeDefinition() != typeof(ChangeStreamDocument<>))
                return null;

            var attribute = parameter.GetCustomAttribute<MongoDbTriggerAttribute>(inherit: false);

            if (attribute == null)
                return null;

            return new MongoDbTriggerBinding(_collectionFactory);
        }
    }
}
