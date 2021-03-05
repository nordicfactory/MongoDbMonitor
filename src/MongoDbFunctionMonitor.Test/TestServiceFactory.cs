using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AccountManagerDbCollectionMonitor.Commands.ProcessAccount;
using AccountManagerDbCollectionMonitor.Commands.ProcessAccountInfo;
using BannerflowDbCollectionMonitor.Commands.ProcessBrand;
using BannerflowDbCollectionMonitor.Commands.ProcessFeed;
using BannerflowDbCollectionMonitor.Commands.ProcessFolder;
using BannerflowDbCollectionMonitor.Commands.ProcessLocalization;
using BannerflowDbCollectionMonitor.Commands.ProcessSizeFormat;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDbCollectionMonitor;
using MongoDbCollectionMonitor.Clients.SlackApi;
using MongoDbCollectionMonitor.Clients.StudioApi;
using MongoDbCollectionMonitor.Commands.Common.Behaviors;
using MongoDbCollectionMonitor.Commands.Common.Responses;
using MongoDbCollectionMonitor.Commands.NotifyStudio;
using MongoDbCollectionMonitor.Commands.ProcessMongoEvent;
using MongoDbCollectionMonitor.Commands.ResolveCollectionType;
using MongoDbCollectionMonitor.Commands.SendSlackAlert;
using MongoDbCollectionMonitor.CrossCutting.QoS;
using MongoDbMonitor.Test.Data;
using Moq;
using Moq.Protected;
using static MongoDbCollectionMonitor.WebJobExtensions;

namespace MongoDbMonitor.Test
{
    internal static class TestServiceFactory
    {
        internal static HttpClient GetMockHttpClient()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.OK));

            return new HttpClient(mockHttpMessageHandler.Object);
        }

        public static IServiceCollection RegisterServices(
            bool includeExceptionHandler = false,
            bool mockHttpClients = false,
            string jsonSettingsName = "test.json")
        {
            var services = new ServiceCollection();

            services.AddSingleton(BuildConfiguration(jsonSettingsName));

            RegisterOptions<Collection<CollectionOptions>>(services, "AppSettings:CollectionOptions");
            RegisterOptions<RetryProviderOptions>(services, "AppSettings:RetryProviderOptions");
            RegisterOptions<StudioApiClientOptions>(services, "AppSettings:StudioApiClientOptions");
            RegisterOptions<SlackApiClientOptions>(services, "AppSettings:SlackApiClientOptions");

            services.AddLogging(x => x.AddConsole());

            services.AddMemoryCache();

            if (mockHttpClients)
            {
                services.AddTransient<IStudioApiClient>(x => new StudioApiClient(
                    x.GetRequiredService<IOptions<StudioApiClientOptions>>(),
                    GetMockHttpClient(),
                    x.GetRequiredService<IRetryProvider>()));

                services.AddTransient<ISlackApiClient>(x => new SlackApiClient(
                    x.GetRequiredService<IOptions<SlackApiClientOptions>>(),
                    GetMockHttpClient(),
                    x.GetRequiredService<IRetryProvider>()));
            }
            else
            {
                services.AddHttpClient<IStudioApiClient, StudioApiClient>(x => GetMockHttpClient());
                services.AddHttpClient<ISlackApiClient, SlackApiClient>(x => GetMockHttpClient());
            }


            services.AddSingleton<IRetryProvider, RetryProvider>();

            services.AddTransient<MonitorRunner>();

            services.AddTransient<IRequestHandler<ProcessMongoEventRequest, ProcessingStatusResponse>, ProcessMongoEventHandler>();
            services.AddTransient<IRequestHandler<ResolveCollectionTypeRequest, ProcessingStatusResponse>, ResolveCollectionTypeHandler>();
            services.AddTransient<IRequestHandler<NotifyStudioRequest, ProcessingStatusResponse>, NotifyStudioHandler>();
            services.AddTransient<IRequestHandler<SendSlackAlertRequest, ProcessingStatusResponse>, SendSlackAlertHandler>();
            services.AddTransient<IRequestHandler<ProcessAccountEventRequest, ProcessingStatusResponse>, ProcessAccountEventHandler>();
            services.AddTransient<IRequestHandler<ProcessAccountInfoEventRequest, ProcessingStatusResponse>, ProcessAccountInfoEventHandler>();
            services.AddTransient<IRequestHandler<ProcessBrandEventRequest, ProcessingStatusResponse>, ProcessBrandEventHandler>();
            services.AddTransient<IRequestHandler<ProcessFolderEventRequest, ProcessingStatusResponse>, ProcessFolderEventHandler>();
            services.AddTransient<IRequestHandler<ProcessFeedEventRequest, ProcessingStatusResponse>, ProcessFeedEventHandler>();
            services.AddTransient<IRequestHandler<ProcessLocalizationEventRequest, ProcessingStatusResponse>, ProcessLocalizationEventHandler>();
            services.AddTransient<IRequestHandler<ProcessSizeFormatEventRequest, ProcessingStatusResponse>, ProcessSizeFormatEventHandler>();
            services.AddTransient<IRequestHandler<InvalidRequest, ProcessingStatusResponse>, InvalidRequestHandler>();


            RegisterMediator(services, ServiceLifetime.Transient);

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(MetricsCapturingPipelineBehavior<,>));

            RegisterMediatorBehaviors(services);

            if (includeExceptionHandler)
            {
                RegisterExtractProcessDocumentExceptionHandlers<ProcessAccountEventRequest, ProcessAccountEventHandler>(services);
                RegisterExtractProcessDocumentExceptionHandlers<ProcessAccountInfoEventRequest, ProcessAccountInfoEventHandler>(services);
                RegisterExtractProcessDocumentExceptionHandlers<ProcessBrandEventRequest, ProcessBrandEventHandler>(services);
                RegisterExtractProcessDocumentExceptionHandlers<ProcessFolderEventRequest, ProcessFolderEventHandler>(services);
                RegisterExtractProcessDocumentExceptionHandlers<ProcessFeedEventRequest, ProcessFeedEventHandler>(services);
                RegisterExtractProcessDocumentExceptionHandlers<ProcessLocalizationEventRequest, ProcessLocalizationEventHandler>(services);
                RegisterExtractProcessDocumentExceptionHandlers<ProcessSizeFormatEventRequest, ProcessSizeFormatEventHandler>(services);

                RegisterMediatorExceptionHandlers(services);
            }

            return services;
        }

        private static IConfiguration BuildConfiguration(string jsonSettingsName = "test.json")
        {
            var builder = new ConfigurationBuilder();

            builder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), jsonSettingsName), false);

            return builder.Build();
        }
    }
}
