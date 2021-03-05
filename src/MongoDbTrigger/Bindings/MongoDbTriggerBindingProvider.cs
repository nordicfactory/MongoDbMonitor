using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MongoDbTrigger.Triggers;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace MongoDbTrigger.Bindings
{
    internal sealed class MongoDbTriggerBindingProvider : ITriggerBindingProvider
    {
        private readonly IConfiguration _configuration;

        public MongoDbTriggerBindingProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context) => Task.FromResult(TryCreate(context));

        public ITriggerBinding TryCreate(TriggerBindingProviderContext context)
        {
            var parameter = context?.Parameter ?? throw new ArgumentNullException(nameof(context));
            if (parameter.ParameterType.GetGenericTypeDefinition() != typeof(ChangeStreamDocument<>))
                return null;

            var attr = parameter.GetCustomAttribute<MongoDbTriggerAttribute>(inherit: false);
            if (attr == null)
                return null;

            var triggerConnectionString = ResolveAttributeConnectionString(attr);

            return new MongoDbTriggerBinding(attr.Database, attr.Collections, triggerConnectionString);
        }

        private string ResolveAttributeConnectionString(MongoDbTriggerAttribute triggerAttribute) =>
            triggerAttribute.ConnectionString.Contains("%")
                ? ResolveConnectionString(triggerAttribute.ConnectionString.Replace("%", ""), nameof(MongoDbTriggerAttribute.ConnectionString))
                : triggerAttribute.ConnectionString;

        private string ResolveConnectionString(string unresolvedConnectionString, string propertyName)
        {
            if (string.IsNullOrEmpty(unresolvedConnectionString)) return null;

            var resolvedString = _configuration.GetConnectionStringOrSetting(unresolvedConnectionString);

            if (string.IsNullOrEmpty(resolvedString))
                throw new InvalidOperationException(
                    $"Unable to resolve app setting for property '{nameof(MongoDbTriggerAttribute)}.{propertyName}'. Make sure the app setting exists and has a valid value.");

            return resolvedString;
        }
    }
}
