using System;
using System.Collections.ObjectModel;
using System.Linq;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using MongoDbCollectionMonitor.Clients.SlackApi;
using MongoDbCollectionMonitor.Clients.StudioApi;
using MongoDbCollectionMonitor.Commands.Common.Behaviors;
using MongoDbCollectionMonitor.Commands.Common.ExceptionHandlers;
using MongoDbCollectionMonitor.Commands.Common.ExceptionHandlers.ExtractDocumentIdentifier;
using MongoDbCollectionMonitor.Commands.Common.ExceptionHandlers.NotifyStudio;
using MongoDbCollectionMonitor.Commands.Common.ExceptionHandlers.ResolveCollectionType;
using MongoDbCollectionMonitor.Commands.Common.Exceptions;
using MongoDbCollectionMonitor.Commands.Common.Responses;
using MongoDbCollectionMonitor.Commands.ExtractDocumentIdentifier;
using MongoDbCollectionMonitor.Commands.NotifyStudio;
using MongoDbCollectionMonitor.Commands.ProcessMongoEvent;
using MongoDbCollectionMonitor.Commands.ResolveCollectionType;
using MongoDbCollectionMonitor.Commands.SendSlackAlert;
using MongoDbCollectionMonitor.CrossCutting.QoS;
using MongoDbTrigger;

namespace MongoDbCollectionMonitor
{
    public static class WebJobExtensions
    {
        public static IWebJobsBuilder AddMongoDbCollectionMonitor(this IWebJobsBuilder builder)
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));

            RegisterOptions<Collection<CollectionOptions>>(
                builder.Services,
                "AzureFunctionsJobHost:AppSettings:CollectionOptions");

            RegisterOptions<RetryProviderOptions>(
                builder.Services,
                "AzureFunctionsJobHost:AppSettings:RetryProviderOptions");

            RegisterOptions<StudioApiClientOptions>(
                builder.Services,
                "AzureFunctionsJobHost:AppSettings:StudioApiClientOptions");

            RegisterOptions<SlackApiClientOptions>(
                builder.Services,
                "AzureFunctionsJobHost:AppSettings:SlackApiClientOptions");

            builder.Services.AddLogging(x => x.AddConsole());

            builder.Services.AddMemoryCache();

            builder.Services.AddHttpClient<IStudioApiClient, StudioApiClient>();
            builder.Services.AddHttpClient<ISlackApiClient, SlackApiClient>();

            builder.Services.AddSingleton<IRetryProvider, RetryProvider>();

            builder.Services.AddTransient<MonitorRunner>();

            RegisterRequestHandler<ProcessMongoEventRequest, ProcessMongoEventHandler>(builder.Services);
            RegisterRequestHandler<ResolveCollectionTypeRequest, ResolveCollectionTypeHandler>(builder.Services);
            RegisterRequestHandler<NotifyStudioRequest, NotifyStudioHandler>(builder.Services);
            RegisterRequestHandler<SendSlackAlertRequest, SendSlackAlertHandler>(builder.Services);

            RegisterMediator(builder.Services, ServiceLifetime.Transient);

            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(MetricsCapturingPipelineBehavior<,>));
            RegisterMediatorBehaviors(builder.Services);
            RegisterMediatorExceptionHandlers(builder.Services);


            builder.AddMongoDbTrigger();

            return builder;
        }

        public static IWebJobsBuilder RegisterProcessDocumentMediatorHandler<TRequest, THandler>(this IWebJobsBuilder builder)
            where THandler : class, IRequestHandler<TRequest, ProcessingStatusResponse>
            where TRequest : ExtractDocumentIdentifierRequest, IRequest<ProcessingStatusResponse>
        {
            builder.Services.AddTransient<IRequestHandler<TRequest, ProcessingStatusResponse>, THandler>();

            RegisterExtractProcessDocumentExceptionHandlers<TRequest, THandler>(builder.Services);

            return builder;
        }

        internal static IServiceCollection RegisterExtractProcessDocumentExceptionHandlers<TRequest, THandler>(
            IServiceCollection services)
            where THandler : class, IRequestHandler<TRequest, ProcessingStatusResponse>
            where TRequest : ExtractDocumentIdentifierRequest, IRequest<ProcessingStatusResponse>
        {
            services.AddTransient<
                IRequestExceptionHandler<TRequest, ProcessingStatusResponse, PropertyNotFoundInDocumentException>,
                PropertyNotFoundInDocumentExceptionHandler<TRequest>>();

            services.AddTransient<
                IRequestExceptionHandler<TRequest, ProcessingStatusResponse, InvalidObjectIdException>,
                InvalidObjectIdExceptionHandler<TRequest>>();

            return services;
        }

        internal static IServiceCollection RegisterOptions<TOption>(IServiceCollection services, string path)
            where TOption : class
        {
            services
                .AddOptions<TOption>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.Bind(path, settings);
                });

            return services;
        }

        internal static IServiceCollection RegisterMediator(IServiceCollection services, ServiceLifetime lifetime)
        {
            services.TryAddTransient<ServiceFactory>(p => p.GetService);
            services.TryAdd(new ServiceDescriptor(typeof(IMediator), typeof(Mediator), lifetime));
            services.TryAdd(new ServiceDescriptor(typeof(ISender), sp => sp.GetService<IMediator>(), lifetime));
            services.TryAdd(new ServiceDescriptor(typeof(IPublisher), sp => sp.GetService<IMediator>(), lifetime));

            return services;
        }

        internal static IServiceCollection RegisterMediatorBehaviors(IServiceCollection services)
        {
            // Use TryAddTransientExact (see below), we dó want to register our Pre/Post processor behavior, even if (a more concrete)
            // registration for IPipelineBehavior<,> already exists. But only once.
            TryAddTransientExact(services, typeof(IPipelineBehavior<,>), typeof(RequestPreProcessorBehavior<,>));
            TryAddTransientExact(services, typeof(IPipelineBehavior<,>), typeof(RequestPostProcessorBehavior<,>));
            TryAddTransientExact(services, typeof(IPipelineBehavior<,>), typeof(RequestExceptionActionProcessorBehavior<,>));
            TryAddTransientExact(services, typeof(IPipelineBehavior<,>), typeof(RequestExceptionProcessorBehavior<,>));

            return services;

            static void TryAddTransientExact(IServiceCollection services, Type serviceType, Type implementationType)
            {
                if (services.Any(reg => reg.ServiceType == serviceType && reg.ImplementationType == implementationType))
                    return;

                services.AddTransient(serviceType, implementationType);
            }
        }

        internal static IServiceCollection RegisterRequestHandler<TRequest, THandler>(IServiceCollection services)
            where TRequest : IRequest<ProcessingStatusResponse>
            where THandler : class, IRequestHandler<TRequest, ProcessingStatusResponse>
        {
            services.AddTransient<IRequestHandler<TRequest, ProcessingStatusResponse>, THandler>();

            return services;
        }

        internal static IServiceCollection RegisterMediatorExceptionHandlers(IServiceCollection services)
        {
            services.AddTransient<
                IRequestExceptionHandler<ResolveCollectionTypeRequest, ProcessingStatusResponse, InvalidRequestTypeException>,
                InvalidRequestTypeExceptionHandler>();

            services.AddTransient<
                IRequestExceptionHandler<ResolveCollectionTypeRequest, ProcessingStatusResponse, MissingRequiredPropertyException>,
                MissingRequiredPropertyExceptionHandler>();

            services.AddTransient<
                IRequestExceptionHandler<NotifyStudioRequest, ProcessingStatusResponse, NotifyStudioFailedException>,
                NotifyStudioFailedExceptionHandler>();

            services.AddTransient(typeof(IRequestExceptionHandler<,,>), typeof(GlobalExceptionHandler<,,>));

            return services;
        }
    }
}
