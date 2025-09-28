using System.Net;
using Microsoft.Extensions.Logging;

namespace ACC.Client.Logging;

internal static partial class LoggerExtensions
{
	[LoggerMessage(Level = LogLevel.Information, Message = "Retrieving children for item: Properties = {Properties}")]
	internal static partial void LogInformationRetrieveChildren(this ILogger logger, string[] properties);

	[LoggerMessage(Level = LogLevel.Debug, Message = "Retrieved item: Properties = {Properties}")]
	internal static partial void LogDebugRetrievedItem(this ILogger logger, string[] properties);

	[LoggerMessage(Level = LogLevel.Debug, Message = "Retrieved item: Properties = {Properties}, Count = {Count}")]
	internal static partial void LogInformationRetrievedItemCount(this ILogger logger, string[] properties, int count);

	[LoggerMessage(Level = LogLevel.Information, Message = "Skipping zero object count item: Properties = {Properties}")]
	internal static partial void LogInformationSkipZeroObjectCountItem(this ILogger logger, string[] properties);

	[LoggerMessage(Level = LogLevel.Error, Message = "Request not successful: Type = {Type}, Status: {StatusCode}, Content: {ErrorContent}")]
	internal static partial void LogErrorHttpRequestNotSuccesful(this ILogger logger, string type, string statusCode, string errorContent);

	// TokenService
	[LoggerMessage(Level = LogLevel.Trace, Message = "Retrieved token: ID = {Id}, Refresh After = {RefreshAfter}")]
	internal static partial void LogTraceRetrievedToken(this ILogger logger, string id, DateTime refreshAfter);

	[LoggerMessage(Level = LogLevel.Debug, Message = "Refreshed token: ID = {Id}, Expires In = {ExpiresIn}")]
	internal static partial void LogDebugRefreshedToken(this ILogger logger, string id, int expiresIn);

	[LoggerMessage(Level = LogLevel.Critical, Message = "Failed to refresh token: Status = {StatusCode}, Response = {ErrorContent}")]
	internal static partial void LogCriticalRefreshTokenFailed(this ILogger logger, string statusCode, string errorContent);

	// RetryPolicyFactory
	[LoggerMessage(Level = LogLevel.Warning, Message = "Retrying request: Attempt = {Attempt}, Delay = {Delay}, Status: {StatusCode}")]
	internal static partial void LogWarningHttpRetry(this ILogger logger, int attempt, TimeSpan delay, HttpStatusCode? statusCode);
}
