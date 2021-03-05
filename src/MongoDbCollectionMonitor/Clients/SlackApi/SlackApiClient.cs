using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDbCollectionMonitor.CrossCutting.QoS;

namespace MongoDbCollectionMonitor.Clients.SlackApi
{
    public interface ISlackApiClient
    {
        Task Send(string payload, CancellationToken cancellation);
    }

    public class SlackApiClient : ISlackApiClient
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
        private readonly SlackApiClientOptions _options;

        public SlackApiClient(IOptions<SlackApiClientOptions> options, HttpClient client, IRetryProvider retrier)
        {
            _options = options.Value;
            _client = client;
            _retrier = retrier;
        }

        public async Task Send(string payload, CancellationToken cancellation)
        {
            using var timeoutSource = new CancellationTokenSource(TimeSpan.FromSeconds(_options.TimeoutInSeconds));
            using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutSource.Token, cancellation);


            await
                _retrier.RetryOn<HttpRequestException, HttpResponseMessage>(
                    CheckError,
                    TransientHttpStatusCodePredicate,
                    () => SendRequest(_client, _options, payload, linkedSource.Token));
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
            SlackApiClientOptions options,
            string payload,
            CancellationToken cancellation)
        {
            var request = CreatePostMessage(new Uri(options.ChannelWebhookUrl), payload);

            var response = await client.SendAsync(request, cancellation);

            if (!response.IsSuccessStatusCode)
                ThrowHttpRequestException(response);

            return response;
        }

        private static HttpRequestMessage CreatePostMessage(Uri uri, string payload)
        {
            return new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = uri,
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };
        }
    }
}
