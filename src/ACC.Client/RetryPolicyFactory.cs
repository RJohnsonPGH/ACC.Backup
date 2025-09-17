using System.Net;
using ACC.Client.Logging;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

namespace ACC.Client;

public sealed class RetryPolicyFactory(ILogger<RetryPolicyFactory> logger)
{
	/// <summary>
	/// Creates a retry policy for handling transient HTTP errors and rate limiting.
	/// </summary>
	/// <returns>The created retry policy.</returns>
	public IAsyncPolicy<HttpResponseMessage> CreatePolicy()
	{
		// Handle transient errors
		var transientPolicy = HttpPolicyExtensions
			.HandleTransientHttpError()
			.WaitAndRetryAsync(
				[
					TimeSpan.FromSeconds(1),
					TimeSpan.FromSeconds(5),
					TimeSpan.FromSeconds(10),
				],
				onRetry: (outcome, timespan, retryAttempt, context) =>
				{
					logger.LogWarningHttpRetry(retryAttempt, timespan, outcome.Result?.StatusCode);
				});

		// Handle rate limit errors
		var tooManyRequestsPolicy = Policy<HttpResponseMessage>
			.HandleResult(r => r.StatusCode == HttpStatusCode.TooManyRequests)
			.WaitAndRetryAsync(
				retryCount: 3,
				sleepDurationProvider: (retryAttempt, outcome, context) =>
				{
					// If the Retry-After header is present, use its value (in seconds) for the delay
					if (outcome.Result.Headers.TryGetValues("Retry-After", out var values) &&
						int.TryParse(values.FirstOrDefault(), out var seconds))
					{
						return TimeSpan.FromSeconds(seconds);
					}

					// Otherwise, use exponential backoff
					return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
				},
				onRetryAsync: (outcome, timespan, retryAttempt, context) =>
				{
					logger.LogWarningHttpRetry(retryAttempt, timespan, outcome.Result?.StatusCode);
					return Task.CompletedTask;
				});

		// Combine both policies
		return Policy.WrapAsync(tooManyRequestsPolicy, transientPolicy);
	}
}
