using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using MongoDB.Driver;
using MongoDbTrigger.Listeners;

namespace MongoDbTrigger.Bindings
{
    internal class MongoDbTriggerBinding : ITriggerBinding
    {
        private readonly string _database;
        private readonly string[] _collections;
        private readonly string _connectionString;

        public Type TriggerValueType => typeof(ChangeStreamDocument<dynamic>);

        public IReadOnlyDictionary<string, Type> BindingDataContract { get; } = new Dictionary<string, Type>();

        public MongoDbTriggerBinding(
            string database,
            string[] collections,
            string connectionString)
        {
            _database = database;
            _collections = collections;
            _connectionString = connectionString;
        }

        public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            ITriggerData triggerData =
                new TriggerData(
                    new ValueProvider(value, TriggerValueType),
                    new Dictionary<string, object>());

            return Task.FromResult(triggerData);
        }

        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            return Task.FromResult<IListener>(
                new MongoDbListener(
                    TriggerValueType,
                    _database,
                    _collections,
                    _connectionString,
                    context.Executor));
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            return new MongoDbTriggerParameterDescriptor
            {
                DatabaseName = _database,
                Collections = string.Join(',', _collections)
            };
        }

        private class ValueProvider : IValueProvider
        {
            private readonly object _value;

            public ValueProvider(object value, Type triggerValueType)
            {
                _value = value;
                Type = typeof(ChangeStreamDocument<>).MakeGenericType(triggerValueType);
            }

            public Type Type { get; }

            public Task<object> GetValueAsync() => Task.FromResult(_value);

            public string ToInvokeString() => DateTime.Now.ToString("o");
        }
    }
}
