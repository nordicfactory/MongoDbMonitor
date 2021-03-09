using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;

namespace MongoDbCollectionMonitor.CrossCutting.QoS
{
    internal interface IRetryProvider
    {
        Task<TResult> RetryOn<TException, TResult>(
            Func<TException, bool> exceptionPredicate,
            Func<TResult, bool> resultPredicate,
            Func<Task<TResult>> execute)
            where TException : Exception;
    }

    internal class RetryProvider : IRetryProvider
    {
        private const string RetryAttemptLogMessage = "Retry attempt: {0}";

        private static readonly Func<int, double> CalculateJitter = delegate (int jitterMaximum)
        {
            var jitter = TimeSpan.FromMilliseconds(new Random().Next(0, jitterMaximum)).TotalMilliseconds;

            return jitter;
        };

        private readonly ILogger _logger;

        internal RetryProviderOptions Options { get; }

        public RetryProvider(IOptionsMonitor<RetryProviderOptions> options, ILogger<RetryProvider> logger)
        {
            Options = options?.CurrentValue ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<TResult> RetryOn<TException, TResult>(
            Func<TException, bool> exceptionPredicate,
            Func<TResult, bool> resultPredicate,
            Func<Task<TResult>> execute)
            where TException : Exception
        {
            return
                Policy
                    .Handle<TException>(exceptionPredicate)
                    .OrResult(resultPredicate)
                    .WaitAndRetryAsync(
                        Options.Delays.Count,
                        i =>
                            {
                                _logger.LogInformation(RetryAttemptLogMessage, new object[1] { i });

                                var delay = Options.Delays[i - 1] + CalculateJitter(Options.JitterMaximum);

                                return TimeSpan.FromMilliseconds(delay);
                            })
                    .ExecuteAsync(execute);
        }
    }
}
