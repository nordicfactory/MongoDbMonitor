using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDbTrigger.DataAccess;
using MongoDbTrigger.Extensions;

namespace MongoDbTrigger
{
    public static class WebjobsBuildExtensions
    {
        /// <summary>
        /// Adds the MongoDbTrigger extension to the provided <see cref="IWebJobsBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IWebJobsBuilder"/> to configure.</param>
        public static IWebJobsBuilder AddMongoDbTrigger(this IWebJobsBuilder builder)
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));

            ConfigureMongoDbTrigger(builder.Services);

            builder.AddExtension<MongoDbTriggerExtensionsConfigProvider>();

            return builder;
        }

        internal static IServiceCollection ConfigureMongoDbTrigger(IServiceCollection services)
        {
            services
                .AddOptions<MongoDbTriggerOptions>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    settings.ConnectionString = configuration.GetSection("AzureFunctionsJobHost:AppSettings:ConnectionString").Get<string>();
                    settings.Database = configuration.GetSection("AzureFunctionsJobHost:AppSettings:Database").Get<string>();

                    int index = 0;

                    while (true)
                    {
                        var collectionName = configuration
                            .GetSection("AzureFunctionsJobHost:AppSettings:CollectionOptions")
                            .GetSection($"{index}:Name").Get<string>();

                        if (string.IsNullOrWhiteSpace(collectionName))
                            break;

                        settings.Collections.Add(collectionName);

                        index++;
                    }

                    configuration.Bind(settings);
                });

                services.AddSingleton<MongoDbCollectionFactory>();

                return services;
        }
    }
}