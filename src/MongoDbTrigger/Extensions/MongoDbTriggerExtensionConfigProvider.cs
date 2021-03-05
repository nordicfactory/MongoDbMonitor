using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Configuration;
using MongoDbTrigger.Bindings;
using MongoDbTrigger.Triggers;

namespace MongoDbTrigger.Extensions
{
    [Extension("MongoDb")]
    internal sealed class MongoDbTriggerExtensionConfigProvider : IExtensionConfigProvider
    {
        private readonly IConfiguration _configuration;

        public MongoDbTriggerExtensionConfigProvider(IConfiguration configuration) => _configuration = configuration;

        /// <summary>
        /// This callback is invoked by the WebJobs framework before the host starts execution.
        /// It should add the binding rules and converters for our new <see cref="MongoDbTriggerAttribute"/>
        /// </summary>
        public void Initialize(ExtensionConfigContext context) =>
            context
                .AddBindingRule<MongoDbTriggerAttribute>()
                .BindToTrigger(new MongoDbTriggerBindingProvider(_configuration));
    }
}
