using System.Net;
using Microsoft.Extensions.Logging;

namespace ACC.Backup.Core.Logging;

internal static partial class LoggerExtensions
{
	[LoggerMessage(Level = LogLevel.Information, Message = "Starting child enumeration: Type = {Type}, Name = {Name}, ID = {Id}")]
	internal static partial void LogInformationChildEnumerationStart(this ILogger logger, string type, string name, string id);

	[LoggerMessage(Level = LogLevel.Information, Message = "Completed child enumeration: Type = {Type}, Name = {Name}, ID = {Id}")]
	internal static partial void LogInformationChildEnumerationComplete(this ILogger logger, string type, string name, string id);

	[LoggerMessage(Level = LogLevel.Information, Message = "Completed enumeration: Type = {Type}")]
	internal static partial void LogInformationEnumerationComplete(this ILogger logger, string type);

	[LoggerMessage(Level = LogLevel.Information, Message = "Item excluded: ID = {Id}, Name = {Name}")]
	internal static partial void LogInformationItemExcluded(this ILogger logger, string id, string name);

	[LoggerMessage(Level = LogLevel.Trace, Message = "Enumerated: Type = {Type}, Name = {Name}, ID = {Id}")]
	internal static partial void LogTraceItemEnumerated(this ILogger logger, string type, string id, string name);

	[LoggerMessage(LogLevel.Debug, "Item not included by job configuration: ID = {Id}")]
	internal static partial void LogDebugIdNotIncluded(this ILogger logger, string id);

	[LoggerMessage(LogLevel.Debug, "Item included by job configuration: ID = {Id}")]
	internal static partial void LogDebugIdIncluded(this ILogger logger, string id);

	[LoggerMessage(LogLevel.Debug, "Item excluded by job configuration: ID = {Id}")]
	internal static partial void LogDebugIdExcluded(this ILogger logger, string id);

	[LoggerMessage(LogLevel.Error, "Download URI missing: ID = {Id}, Name = {Name}")]
	internal static partial void LogErrorNoDownloadUri(this ILogger logger, string id, string name);

	[LoggerMessage(LogLevel.Error, "Download skipped, repository is up to date: ID = {Id}, Name = {Name}, Version = {Version}")]
	internal static partial void LogTraceDownloadSkipped(this ILogger logger, string id, string name, int version);

	[LoggerMessage(LogLevel.Error, "Download failed: ID = {Id}, Name = {Name}, Version = {Version}")]
	internal static partial void LogErrorFileDownloadFailed(this ILogger logger, string id, string name, int version);

	[LoggerMessage(LogLevel.Debug, "Completed downloading: ID = {Id}, Name = {Name}, Version = {Version}")]
	internal static partial void LogDebugFileDownloadComplete(this ILogger logger, string id, string name, int version);

	[LoggerMessage(LogLevel.Error, "All downloads are complete.")]
	internal static partial void LogInformationDownloadComplete(this ILogger logger);

	// Repository
	[LoggerMessage(LogLevel.Trace, Message = "Updated item metadata: Project ID = {ProjectId}, Item ID = {ItemId}, Version = {Version}")]
	internal static partial void LogTraceItemMetadataUpdated(this ILogger logger, string projectId, string itemId, int version);

	[LoggerMessage(LogLevel.Trace, Message = "Successfully downloaded item: Project ID = {ProjectId}, Item ID = {ItemId}, Version = {Version}")]
	internal static partial void LogTraceItemDownloadedSuccessfully(this ILogger logger, string projectId, string itemId, int version);

	[LoggerMessage(LogLevel.Error, Message = "Failed to downloaded item: Project ID = {ProjectId}, Item ID = {itemId}, Version = {Version}")]
	internal static partial void LogErrorItemDownloadFailed(this ILogger logger, Exception ex, string projectId, string itemId, int version);

	[LoggerMessage(LogLevel.Trace, Message = "Download item path: Project ID = {ProjectId}, Item ID = {ItemId}, Version = {Version}Path = {Path}")]
	internal static partial void LogTraceItemDownloadDestination(this ILogger logger, string projectId, string itemId, int version, string path);

	[LoggerMessage(LogLevel.Trace, Message = "Item repository state: Project ID = {ProjectId}, Item ID = {ItemId}, Version = {Version}")]
	internal static partial void LogTraceItemRepositoryVersion(this ILogger logger, string projectId, string itemId, int version);

	[LoggerMessage(Level = LogLevel.Error, Message = "Request not successful: Project ID = {ProjectId}, Item ID = {ItemId}, Version = {Version}, Status = {StatusCode}")]
	internal static partial void LogErrorHttpRequestNotSuccesful(this ILogger logger, string projectId, string itemId, int version, HttpStatusCode statusCode);

	[LoggerMessage(Level = LogLevel.Trace, Message = "Item file size: Project ID = {ProjectId}, Item ID = {ItemId}, Version = {Version}, Size = {Size}")]
	internal static partial void LogTraceBackupFileSize(this ILogger logger, string ProjectId, string itemId, int version, long size);

	// Report
	[LoggerMessage(Level = LogLevel.Error, Message = "Report key not found: Type = {Type}, ID = {Id}")]
	internal static partial void LogErrorReportKeyNotFound(this ILogger logger, string type, string id);
}
