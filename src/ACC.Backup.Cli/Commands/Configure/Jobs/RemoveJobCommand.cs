using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using Spectre.Console;
using System.ComponentModel;
using ACC.Backup.Cli.Data;
using ACC.Backup.Cli.Logging;

namespace ACC.Backup.Cli.Commands.Configure.Jobs;

public sealed partial class RemoveJobCommand(ILogger<RemoveJobCommand> logger, ConfigurationDbContext dbContext) 
	: AsyncCommand<RemoveJobCommand.Settings>
{
	public sealed class Settings : CommandSettings
	{
		[CommandOption("-j|--job", true)]
		[Description("The id of the backup job to remove from the configuration.")]
		public required int JobId { get; init; }
	}

	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
	{
		logger.LogInformationCommandStart("RemoveJobCommand", [$"settings.JobId"]);
		try
		{
			var job = await dbContext.Jobs.FindAsync(settings.JobId)
				?? throw new InvalidOperationException("No job found for supplied job ID.");

			dbContext.Jobs.Remove(job);
			await dbContext.SaveChangesAsync();
			logger.LogTraceDatabaseOperationSuccessful();
		}
		catch (Exception ex)
		{
			logger.LogCriticalDatabaseOperationFailed(ex);
			AnsiConsole.MarkupLineInterpolated($"[red]Job '{settings.JobId}' could not be removed from the configuration. See log for details.[/]");
			return 1;
		}

		AnsiConsole.MarkupLineInterpolated($"[green]Job '{settings.JobId}' removed successfully.[/]");
		return 0;
	}
}
