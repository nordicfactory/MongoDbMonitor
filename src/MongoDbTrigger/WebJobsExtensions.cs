using System;
using Microsoft.Azure.WebJobs;
using MongoDbTrigger.Extensions;

namespace MongoDbTrigger
{
    public static class WebJobsExtensions
    {
        /// <summary>
        /// Adds the MongoDbTrigger extension to the provided <see cref="IWebJobsBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IWebJobsBuilder"/> to configure.</param>
        public static IWebJobsBuilder AddMongoDbTrigger(this IWebJobsBuilder builder)
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));

            builder.AddExtension<MongoDbTriggerExtensionConfigProvider>();

            return builder;
        }
    }
}
