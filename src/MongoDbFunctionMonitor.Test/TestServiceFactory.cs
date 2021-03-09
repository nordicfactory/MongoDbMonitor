using System;
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
using MongoDbCollectionMonitor.Commands.Common.Responses;
using MongoDbMonitor.Test.Data;
using MongoDbTrigger;
using Moq;
using Moq.Protected;
using static MongoDbCollectionMonitor.WebJobExtensions;

namespace MongoDbMonitor.Test
{
    internal static class TestServiceFactory
    {
        public static IServiceCollection RegisterServices(
            bool includeExceptionHandler = false,
            bool mockHttpClients = false,
            string jsonSettingsName = "test.json")
        {
            var services = new ServiceCollection();

            services.AddSingleton(BuildConfiguration(jsonSettingsName));

            // Register concrete handlers
            services.AddTransient<IRequestHandler<ProcessAccountEventRequest, ProcessingStatusResponse>, ProcessAccountEventHandler>();
            services.AddTransient<IRequestHandler<ProcessAccountInfoEventRequest, ProcessingStatusResponse>, ProcessAccountInfoEventHandler>();
            services.AddTransient<IRequestHandler<ProcessBrandEventRequest, ProcessingStatusResponse>, ProcessBrandEventHandler>();
            services.AddTransient<IRequestHandler<ProcessFolderEventRequest, ProcessingStatusResponse>, ProcessFolderEventHandler>();
            services.AddTransient<IRequestHandler<ProcessFeedEventRequest, ProcessingStatusResponse>, ProcessFeedEventHandler>();
            services.AddTransient<IRequestHandler<ProcessLocalizationEventRequest, ProcessingStatusResponse>, ProcessLocalizationEventHandler>();
            services.AddTransient<IRequestHandler<ProcessSizeFormatEventRequest, ProcessingStatusResponse>, ProcessSizeFormatEventHandler>();
            services.AddTransient<IRequestHandler<InvalidRequest, ProcessingStatusResponse>, InvalidRequestHandler>();

            if (includeExceptionHandler)
            {
                // Register concrete exception handlers before RegisterMonitorService registers generic ones
                RegisterExtractProcessDocumentExceptionHandlers<ProcessAccountEventRequest, ProcessAccountEventHandler>(services);
                RegisterExtractProcessDocumentExceptionHandlers<ProcessAccountInfoEventRequest, ProcessAccountInfoEventHandler>(services);
                RegisterExtractProcessDocumentExceptionHandlers<ProcessBrandEventRequest, ProcessBrandEventHandler>(services);
                RegisterExtractProcessDocumentExceptionHandlers<ProcessFolderEventRequest, ProcessFolderEventHandler>(services);
                RegisterExtractProcessDocumentExceptionHandlers<ProcessFeedEventRequest, ProcessFeedEventHandler>(services);
                RegisterExtractProcessDocumentExceptionHandlers<ProcessLocalizationEventRequest, ProcessLocalizationEventHandler>(services);
                RegisterExtractProcessDocumentExceptionHandlers<ProcessSizeFormatEventRequest, ProcessSizeFormatEventHandler>(services);
            }

            RegisterMonintorService(
                services,
                mockHttpClients ? GetMockHttpClient : NullClient,
                mockHttpClients ? GetMockHttpClient : NullClient,
                includeExceptionHandler);


            return services;
            
            
        }

        private static Func<HttpClient> NullClient = () => null;

        public static IServiceCollection RegisterMongoDbTrigger(string jsonSettingsName)
        {
            var services = new ServiceCollection();

            services.AddSingleton(BuildConfiguration(jsonSettingsName));

            WebjobsBuildExtensions.ConfigureMongoDbTrigger(services);

            return services;
        }

        private static IConfiguration BuildConfiguration(string jsonSettingsName = "test.json")
        {
            var builder = new ConfigurationBuilder();

            builder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), jsonSettingsName), false);

            return builder.Build();
        }

        private static HttpClient GetMockHttpClient()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.OK));

            return new HttpClient(mockHttpMessageHandler.Object);
        }
    }
}
