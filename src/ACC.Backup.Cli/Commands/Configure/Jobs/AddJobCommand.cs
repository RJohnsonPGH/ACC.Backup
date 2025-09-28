using System.ComponentModel;
using ACC.Backup.Cli.Data;
using ACC.Backup.Cli.Logging;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ACC.Backup.Cli.Commands.Configure.Jobs;

public sealed partial class AddJobCommand(ILogger<AddJobCommand> logger, ConfigurationDbContext dbContext) 
	: AsyncCommand<AddJobCommand.Settings>
{
	public sealed class Settings : CommandSettings
	{
		[CommandOption("-p|--path", true)]
		[Description("The path to store backups.")]
		public required string Path { get; init; }

		[CommandOption("-i|--include")]
		[Description("The Id of a hub or project to be included in the backup job. If not supplied, all non-excluded hubs and projects will be backed up. This option can be specified multiple times.")]
		public string[] IncludeIds { get; init; } = [];

		[CommandOption("-e|--exclude")]
		[Description("The Id of a hub or project to be excluded from the backup job. Exclusions will override inclusions. This option can be specified multiple times.")]
		public string[] ExcludeIds { get; init; } = [];
	}

	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
	{
		logger.LogInformationCommandStart("AddJobCommand", [settings.Path, $"{settings.IncludeIds.Length}", $"{settings.ExcludeIds.Length}"]);

		var newJob = new Job()
		{
			StoragePath = settings.Path,
			IncludeIds = settings.IncludeIds,
			ExcludeIds = settings.ExcludeIds,
		};
		try
		{
			await dbContext.Jobs.AddAsync(newJob);
			await dbContext.SaveChangesAsync();
			logger.LogTraceDatabaseOperationSuccessful();
		}
		catch (Exception ex)
		{
			logger.LogCriticalDatabaseOperationFailed(ex);
			AnsiConsole.MarkupLineInterpolated($"[red]Job could not be added to the configuration. See log for details.[/]");
			return 1;
		}

		AnsiConsole.MarkupLineInterpolated($"[green]Job '{newJob.Id}' added successfully.[/]");
		return 0;
	}
}
