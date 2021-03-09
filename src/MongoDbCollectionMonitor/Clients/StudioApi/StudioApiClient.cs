using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDbCollectionMonitor.CrossCutting.QoS;

namespace MongoDbCollectionMonitor.Clients.StudioApi
{
    internal interface IStudioApiClient
    {
        Task NotifyStudio(StudioCacheType cacheType, ObjectId id, CancellationToken cancellation);
    }

    internal class StudioApiClient : IStudioApiClient
    {
        private static readonly Func<HttpResponseMessage, bool> TransientHttpStatusCodePredicate =
            delegate (HttpResponseMessage response)
            {
                if (response.StatusCode < HttpStatusCode.InternalServerError)
                    return response.StatusCode == HttpStatusCode.RequestTimeout;

                return true;
            };

        private static readonly Action<HttpResponseMessage> ThrowHttpRequestException = delegate (HttpResponseMessage response)
        {
            throw new HttpRequestException(response.ReasonPhrase) { Data = { [nameof(HttpStatusCode)] = response.StatusCode } };
        };

        private readonly IRetryProvider _retrier;
        private readonly HttpClient _client;

        internal StudioApiClientOptions Options { get; }

        public StudioApiClient(IOptions<StudioApiClientOptions> options, HttpClient client, IRetryProvider retrier)
        {
            Options = options.Value;
            _client = client;
            _retrier = retrier;
        }

        public async Task NotifyStudio(StudioCacheType cacheType, ObjectId id, CancellationToken cancellation)
        {
            using var timeoutSource = new CancellationTokenSource(TimeSpan.FromSeconds(Options.TimeoutInSeconds * 2));
            using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutSource.Token, cancellation);

            var response =
                await _retrier
                    .RetryOn<HttpRequestException, HttpResponseMessage>(
                        CheckError,
                        TransientHttpStatusCodePredicate,
                        () => SendRequest(_client, cacheType, id, Options, linkedSource.Token));

            if (!response.IsSuccessStatusCode)
                ThrowHttpRequestException(response);
        }

        private static bool CheckError(HttpRequestException x)
        {
            if (!x.Data.Contains(nameof(HttpStatusCode)))
                return false;

            var statusCode = (HttpStatusCode)x.Data[nameof(HttpStatusCode)];

            if (statusCode < HttpStatusCode.InternalServerError)
                return statusCode == HttpStatusCode.RequestTimeout;

            return false;
        }

        private static async Task<HttpResponseMessage> SendRequest(
            HttpClient client,
            StudioCacheType cacheType,
            ObjectId id,
            StudioApiClientOptions options,
            CancellationToken cancellation)
        {
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutInSeconds);

            var body = $"{{\"type\": {(int) cacheType}, \"cacheKey\": \"{id}\"}}";

            var message = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
                RequestUri = options.ClearCacheWebhook
            };

            message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.SendAsync(message, cancellation);

            return response;
        }
    }
}
