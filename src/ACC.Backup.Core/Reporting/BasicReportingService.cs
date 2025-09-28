using ACC.Backup.Core.Logging;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using table.lib;

namespace ACC.Backup.Core.Reporting;

/// <summary>
/// A basic implementation of IReportingService that generates HTML tables.
/// </summary>
/// <param name="logger"></param>
public sealed class BasicReportingService(ILogger<BasicReportingService> logger) : IReportingService
{
	// Store incoming data in thread-safe collections
	private readonly ConcurrentDictionary<string, HubRecord> _hubs = new();
	private readonly ConcurrentDictionary<string, ProjectRecord> _projects = new();
	private readonly ConcurrentBag<FileRecord> _files = [];
	private readonly ConcurrentBag<string> _messages = [];

	// HTML formatting
	private readonly static string DivStart = @"<div style=""margin-top: 1em; margin-bottom: 1em; margin-left: auto; margin-right: auto; width: 80%;"">";
	private readonly static string DivEnd = "</div>";

	/// <summary>
	/// Add a hub to the report.
	/// </summary>
	/// <param name="id">The ID of the hub.<param>
	/// <param name="name">The name of the hub.</param>
	/// <param name="isExcluded">If this hub is excluded by the backup job settings.</param>
	public void AddHub(string id, string name, bool isExcluded = false)
	{
		_hubs[id] = new HubRecord(id, name, isExcluded);
	}

	/// <summary>
	/// Add a project to the report.
	/// </summary>
	/// <param name="id">The ID of the project.<param>
	/// <param name="name">The name of the project.</param>
	/// <param name="hubId">The ID of the hub that contains this project.</param>
	/// <param name="isExcluded">If this project is excluded by the backup job settings.</param>
	public void AddProject(string id, string name, string hubId, bool isExcluded = false)
	{
		_projects[id] = new ProjectRecord(id, hubId, name, isExcluded);
	}

	/// <summary>
	/// Add a file to the report.
	/// </summary>
	/// <param name="id">The ID of the file.<param>
	/// <param name="name">The name of the file.</param>
	/// <param name="projectId">The ID of the project that contains this file.</param>
	/// <param name="state">The state of the file download.</param>
	public void AddFile(string id, string name, string projectId, ReportingState state)
	{
		_files.Add(new FileRecord(id, projectId, name, state));
	}

	/// <summary>
	/// Add a message to the report.
	/// </summary>
	/// <param name="message">The message that will be added to the report.</param>
	public void AddMessage(string message)
	{
		_messages.Add(message);
	}

	/// <summary>
	/// Generate the HTML report.
	/// </summary>
	/// <returns>The HTML tables that represent the backup state.</returns>
	public string GenerateReport()
	{
		var reportProjects = _files
			.GroupBy(x => x.ProjectId)
			.Select(x =>
			{
				_projects.TryGetValue(x.Key, out var project);
				if (project is null)
				{
					logger.LogErrorReportKeyNotFound("Project", x.Key);
				}

				_hubs.TryGetValue(project?.HubId ?? string.Empty, out var hub);
				if (hub is null)
				{
					logger.LogErrorReportKeyNotFound("Hub", x.Key);
				}

				return new ReportProject(
					HubName: hub?.Name ?? "Unknown Hub",
					ProjectName: project?.Name ?? "Unknown Project",
					TotalFiles: x.Count(),
					UpToDateFiles: x.Count(x => x.State == ReportingState.UpToDate),
					SuccessfulFiles: x.Count(x => x.State == ReportingState.Successful),
					FailedFiles: x.Count(x => x.State == ReportingState.Failed)
				);
			})
			.ToList();

		var reportSummary = new ReportSummary(
			TotalFiles: _files.Count,
			UpToDateFiles: _files.Count(x => x.State == ReportingState.UpToDate),
			SuccessfulFiles: _files.Count(x => x.State == ReportingState.Successful),
			FailedFiles: _files.Count(x => x.State == ReportingState.Failed)
		);

		var reportHubExclusions = _hubs
			.Where(x => x.Value.IsExcluded)
			.Select(x => new ReportExclusion(x.Value.Id, "Hub", x.Value.Name));

		var reportProjectExclusions = _projects
			.Where(x => x.Value.IsExcluded)
			.Select(x => new ReportExclusion(x.Value.Id, "Project", x.Value.Name));

		var reportExclusions = reportHubExclusions
			.Concat(reportProjectExclusions)
			.ToList();

		var reportMessages = _messages
			.Select(x => new ReportMessage(x))
			.ToList();

		// Generate and combine the HTML tables
		var reportSummaryHtml = new Table<ReportSummary>([reportSummary])
			.ToHtml();
		var reportProjectsHtml = new Table<ReportProject>(reportProjects)
			.ToHtml();
		var reportExclusionsHtml = new Table<ReportExclusion>(reportExclusions)
			.ToHtml();
		var reportMessagesHtml = new Table<ReportMessage>(reportMessages)
			.ToHtml();

		return string.Join(null, 
			$"{DivStart}{reportSummaryHtml}{DivEnd}", 
			$"{DivStart}{reportProjectsHtml}{DivEnd}",
			$"{DivStart}{reportExclusionsHtml}{DivEnd}",
			$"{DivStart}{reportMessagesHtml}{DivEnd}");
	}
}

/// <summary>
/// An exclusion to be included in the report.
/// </summary>
/// <param name="ExcludedId"></param>
/// <param name="Type"></param>
/// <param name="Name"></param>
internal sealed record ReportExclusion(
	string ExcludedId,
	string Type,
	string Name
);

/// <summary>
/// A summary of the backup operation.
/// </summary>
/// <param name="TotalFiles"></param>
/// <param name="UpToDateFiles"></param>
/// <param name="SuccessfulFiles"></param>
/// <param name="FailedFiles"></param>
internal sealed record ReportSummary(
	int TotalFiles,
	int UpToDateFiles,
	int SuccessfulFiles,
	int FailedFiles
);

/// <summary>
/// A report for a specific project.
/// </summary>
/// <param name="ProjectName"></param>
/// <param name="HubName"></param>
/// <param name="TotalFiles"></param>
/// <param name="UpToDateFiles"></param>
/// <param name="SuccessfulFiles"></param>
/// <param name="FailedFiles"></param>
internal sealed record ReportProject(
	string ProjectName,
	string HubName,
	int TotalFiles,
	int UpToDateFiles,
	int SuccessfulFiles,
	int FailedFiles
);

/// <summary>
/// An message to be included in the report.
/// </summary>
/// <param name="Message"></param>
internal sealed record ReportMessage(string Message);

/// <summary>
/// A record representing a file and its state.
/// </summary>
/// <param name="Id"></param>
/// <param name="ProjectId"></param>
/// <param name="Name"></param>
/// <param name="State"></param>
internal sealed record FileRecord(string Id, string ProjectId, string Name, ReportingState State);

/// <summary>
/// A record representing a hub.
/// </summary>
/// <param name="Id"></param>
/// <param name="Name"></param>
/// <param name="IsExcluded"></param>
internal sealed record HubRecord(string Id, string Name, bool IsExcluded);

/// <summary>
///	A record representing a project.
/// </summary>
/// <param name="Id"></param>
/// <param name="HubId"></param>
/// <param name="Name"></param>
/// <param name="IsExcluded"></param>
internal sealed record ProjectRecord(string Id, string HubId, string Name, bool IsExcluded);
