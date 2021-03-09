using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        public static IWebJobsBuilder RegisterProcessDocumentMediatorHandler<TRequest, THandler>(this IWebJobsBuilder builder)
            where THandler : class, IRequestHandler<TRequest, ProcessingStatusResponse>
            where TRequest : ExtractDocumentIdentifierRequest, IRequest<ProcessingStatusResponse>
        {
            builder.Services.AddTransient<IRequestHandler<TRequest, ProcessingStatusResponse>, THandler>();

            RegisterExtractProcessDocumentExceptionHandlers<TRequest, THandler>(builder.Services);

            return builder;
        }

        public static IWebJobsBuilder AddMongoDbCollectionMonitor(
            this IWebJobsBuilder builder)
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));

            RegisterMonintorService(builder.Services);

            builder.AddMongoDbTrigger();

            return builder;
        }

        internal static IServiceCollection RegisterMonintorService(IServiceCollection services,
            Func<HttpClient> studioHttpClientInstance = null,
            Func<HttpClient> slackHttpClientInstance = null,
            bool useExceptionHandlers = true)
        {
            RegisterMonitorOptions(services);

            services.AddLogging(x => x.AddConsole());
            services.AddMemoryCache();

            var studioClinet = slackHttpClientInstance?.Invoke();
            if (studioClinet == null)
                services.AddHttpClient<IStudioApiClient, StudioApiClient>();
            else
                services.AddTransient<IStudioApiClient>(x => new StudioApiClient(
                    x.GetRequiredService<IOptions<StudioApiClientOptions>>(),
                    studioHttpClientInstance(),
                    x.GetRequiredService<IRetryProvider>()));

            var slackClient = slackHttpClientInstance?.Invoke();
            if (slackClient == null)
                services.AddHttpClient<ISlackApiClient, SlackApiClient>();
            else
                services.AddTransient<ISlackApiClient>(x => new SlackApiClient(
                    x.GetRequiredService<IOptions<SlackApiClientOptions>>(),
                    slackHttpClientInstance(),
                    x.GetRequiredService<IRetryProvider>()));

            services.AddSingleton<IRetryProvider, RetryProvider>();

            services.AddTransient<MonitorRunner>();

            RegisterRequestHandler<ProcessMongoEventRequest, ProcessMongoEventHandler>(services);
            RegisterRequestHandler<ResolveCollectionTypeRequest, ResolveCollectionTypeHandler>(services);
            RegisterRequestHandler<NotifyStudioRequest, NotifyStudioHandler>(services);
            RegisterRequestHandler<SendSlackAlertRequest, SendSlackAlertHandler>(services);

            RegisterMediator(services, ServiceLifetime.Transient);

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(MetricsCapturingPipelineBehavior<,>));
            
            RegisterMediatorBehaviors(services);

            if (useExceptionHandlers)
                RegisterMediatorExceptionHandlers(services);

            return services;
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

        internal static IServiceCollection RegisterMonitorOptions(IServiceCollection services)
        {
            RegisterOptions<Collection<CollectionOptions>>(services, "AzureFunctionsJobHost:AppSettings:CollectionOptions");
            RegisterOptions<RetryProviderOptions>(services, "AzureFunctionsJobHost:AppSettings:RetryProviderOptions");
            RegisterOptions<StudioApiClientOptions>(services, "AzureFunctionsJobHost:AppSettings:StudioApiClientOptions");
            RegisterOptions<SlackApiClientOptions>(services, "AzureFunctionsJobHost:AppSettings:SlackApiClientOptions");

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

        private static IServiceCollection RegisterOptions<TOption>(IServiceCollection services, string path)
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
    }
}
