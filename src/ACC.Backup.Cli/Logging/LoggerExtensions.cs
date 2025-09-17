using Microsoft.Extensions.Logging;

namespace ACC.Backup.Cli.Logging;

internal static partial class LoggerExtensions
{
	[LoggerMessage(Level = LogLevel.Information, Message = "Executing command: Command = {Command}, Settings = {Settings}")]
	internal static partial void LogInformationCommandStart(this ILogger logger, string command, string[] settings);

	[LoggerMessage(Level = LogLevel.Debug, Message = "Successfully modified configuration")]
	internal static partial void LogTraceDatabaseOperationSuccessful(this ILogger logger);

	[LoggerMessage(Level = LogLevel.Critical, Message = "Failed to modifiy configuration")]
	internal static partial void LogCriticalDatabaseOperationFailed(this ILogger logger, Exception exception);

	[LoggerMessage(Level = LogLevel.Information, Message = "Provider loaded configuration: Properties = {Properties}")]
	internal static partial void LogInformationProviderPopulatedConfiguration(this ILogger logger, string[] properties);

	[LoggerMessage(Level = LogLevel.Critical, Message = "Provider failed to load configuration: Properties = {Properties}")]
	internal static partial void LogCriticalProviderConfigurationFailed(this ILogger logger, Exception exception, string[] properties);
}
