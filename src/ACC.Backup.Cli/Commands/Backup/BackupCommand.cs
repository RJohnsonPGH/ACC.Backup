using System.Collections.Concurrent;
using System.ComponentModel;
using ACC.Backup.Cli.Logging;
using ACC.Backup.Core.Backup;
using ACC.Backup.Core.Backup.Progress;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Rendering;

namespace ACC.Backup.Cli.Commands.Backup;

public sealed partial class BackupCommand(ILogger<BackupCommand> logger, IBackupService backupService) 
    : AsyncCommand<BackupCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandOption("-j|--job", true)]
        [Description("The id of the backup job to execute.")]
        public int JobId { get; set; }
    }

	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        logger.LogInformationCommandStart("BackupCommand", [$"{settings.JobId}"]);
        var returnValue = await AnsiConsole.Progress()
            .Columns(
            [
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new ElapsedTimeColumn(),
                new SpinnerColumn(),
            ])
            .HideCompleted(true)
            .UseRenderHook((renderable, tasks) => RenderHook(renderable, tasks))
            .StartAsync(async context => 
            {
                int hubCount = 0;
                int projectCount = 0;
                int fileCount = 0;

                // Display tasks
                var discoverHubsInTenantDisplayTask = context.AddTask("Discovering hubs in tenant", false, 1)
                    .IsIndeterminate();
                var discoverProjectsInHubsDisplayTask = context.AddTask("Discovering projects in hubs", false, 1)
                    .IsIndeterminate();
                var discoverFilesInProjectsDisplayTask = context.AddTask("Discovering files in projects", false, 1)
				    .IsIndeterminate();
                var downloadDisplayTask = context.AddTask("Downloading files", false, 1)
                    .IsIndeterminate();

				// Backup tasks
				var progress = new Progress<DiscoveryProgress>(x =>
                {
                    switch (x)
                    {
                        // Discovery
                        case DiscoveryProgress.HubDiscovered:
							discoverProjectsInHubsDisplayTask.MaxValue = Interlocked.Increment(ref hubCount);
                            break;
                        case DiscoveryProgress.ProjectDiscovered:
							discoverFilesInProjectsDisplayTask.MaxValue = Interlocked.Increment(ref projectCount);
							break;
                        case DiscoveryProgress.FileDiscovered:
							downloadDisplayTask.MaxValue = Interlocked.Increment(ref fileCount);
							break;
						// Enumeration
						case DiscoveryProgress.TenantEnumerated:
							discoverHubsInTenantDisplayTask.Increment(1);
							break;
						case DiscoveryProgress.HubEnumerated:
                            discoverProjectsInHubsDisplayTask.Increment(1);
                            break;
                        case DiscoveryProgress.ProjectEnumerated:
                            discoverFilesInProjectsDisplayTask.Increment(1);
                            break;
						// Complete
						case DiscoveryProgress.HubEnumerationComplete:
							discoverProjectsInHubsDisplayTask.IsIndeterminate(false);
							discoverHubsInTenantDisplayTask.StopTask();
                            _log.Add($"[green]Hub discovery complete. Hubs found: {hubCount}.[/]");
							break;
                        case DiscoveryProgress.ProjectEnumerationComplete:
							discoverFilesInProjectsDisplayTask.IsIndeterminate(false);
							discoverProjectsInHubsDisplayTask.StopTask();
							_log.Add($"[green]Project discovery complete. Projects found: {projectCount}.[/]");
							break;
                        case DiscoveryProgress.FileEnumerationComplete:
							downloadDisplayTask.IsIndeterminate(false);
							discoverFilesInProjectsDisplayTask.StopTask();
							_log.Add($"[green]File discovery complete. Files found: {fileCount}.[/]");
							break;
					};
                });

                discoverHubsInTenantDisplayTask.StartTask();
                discoverProjectsInHubsDisplayTask.StartTask();
                discoverFilesInProjectsDisplayTask.StartTask();
                downloadDisplayTask.StartTask();
				var retrieveHubsTask = backupService.EnumerateHubsAsync(progress);
                var retrieveProjectsTask = backupService.EnumerateProjectsAsync(progress);
                var retrieveFilesTask = backupService.EnumerateFilesAsync(progress);

                var downloadDisplayTasks = new ConcurrentDictionary<string, ProgressTask>();
				var fileDownloadProgress = new Progress<DownloadProgress>(x =>
                {
                    if (!downloadDisplayTasks.TryGetValue(x.Id, out var existingTask))
					{
                        existingTask = context.AddTask($"Downloading: {x.Name}");
                        existingTask.IsIndeterminate(!x.BytesTotal.HasValue);
						downloadDisplayTasks[x.Id] = existingTask;
					}

					switch (x.Status)
                    {
						case DownloadProgress.DownloadStatus.InProgress:
							existingTask.Value = x.PercentComplete;
							break;
						case DownloadProgress.DownloadStatus.Failed:
                            existingTask.Value = 100;
							existingTask.StopTask();
							_log.Add($"[red]Failed to download file:[/] {x.Id} - {x.Name}");
							downloadDisplayTask.Increment(1);
                            break;
						case DownloadProgress.DownloadStatus.Completed:
							existingTask.Value = 100;
							existingTask.StopTask();
							downloadDisplayTask.Increment(1);
							break;
					}
                });
                var downloadFilesTask = backupService.BackupProjectFilesAsync(fileDownloadProgress);

                await Task.WhenAll(retrieveHubsTask, retrieveProjectsTask, retrieveFilesTask, downloadFilesTask);
				await backupService.SaveReportAsync();

				return 0;
			});

		return returnValue;
	}

	private readonly IRenderable[] _logPanelRows = [
		new Markup("[bold]Log[/]"),
		new Rule()
	];
	private readonly List<string> _log = [];
	private Rows RenderHook(IRenderable renderable, IReadOnlyList<ProgressTask> tasks)
	{
        var taskPanelRows = new IRenderable[]
        {
            new Markup($"[bold]ACC Backup[/]"),
            new Rule(),
            renderable,
			new Rule(),
			new Markup($"[bold]Details[/]"),
			new Rule(),
            new Text($"Hubs discovered in tenant: {tasks[1].MaxValue}"),
            new Text($"Projects discovered in hubs: {tasks[2].MaxValue}"),
            new Text($"Files discovered in projects: {tasks[3].MaxValue}"),
		};

        var taskPanel = new Panel(
            new Rows(taskPanelRows)
        );

        var latestLogs = _log.Count > 5 ?
            _log[^5..] :
            _log[0..];

        var logPanel = new Panel(
            new Rows(_logPanelRows
                .Concat(latestLogs.Select(x => 
                    new Markup(x)
                        .Overflow(Overflow.Ellipsis)
                    )
                )
            )
        );

        return new Rows(taskPanel, logPanel);
	}
}