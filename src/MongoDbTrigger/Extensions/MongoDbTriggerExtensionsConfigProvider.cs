﻿using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using MongoDbTrigger.Bindings;
using MongoDbTrigger.DataAccess;
using MongoDbTrigger.Triggers;

namespace MongoDbTrigger.Extensions
{
    [Extension("MongoDb")]
    internal sealed class MongoDbTriggerExtensionsConfigProvider : IExtensionConfigProvider
    {
        private readonly MongoDbCollectionFactory _collectionFactory;

        public MongoDbTriggerExtensionsConfigProvider(MongoDbCollectionFactory collectionFactory)
        {
            _collectionFactory = collectionFactory;
        }

        /// <summary>
        /// This callback is invoked by the WebJobs framework before the host starts execution.
        /// It should add the binding rules and converters for our new <see cref="MongoDbTriggerAttribute"/>
        /// </summary>
        public void Initialize(ExtensionConfigContext context) =>
            context
                .AddBindingRule<MongoDbTriggerAttribute>()
                .BindToTrigger(new MongoDbBindingProvider(_collectionFactory));
    }
}
