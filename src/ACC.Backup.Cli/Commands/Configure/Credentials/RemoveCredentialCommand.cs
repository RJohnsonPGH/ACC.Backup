using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using Spectre.Console;
using System.ComponentModel;
using ACC.Backup.Cli.Data;
using ACC.Backup.Cli.Logging;

namespace ACC.Backup.Cli.Commands.Configure.Credentials;

public sealed partial class RemoveCredentialCommand(ILogger<RemoveCredentialCommand> logger, ConfigurationDbContext dbContext) 
	: AsyncCommand<RemoveCredentialCommand.Settings>
{
	public sealed class Settings : CommandSettings
	{
		[CommandOption("-c|--clientId", true)]
		[Description("The client id of the credential to remove from the configuration.")]
		public required string ClientId { get; init; }
	}

	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
	{
		logger.LogInformationCommandStart("RemoveCredentialCommand", [settings.ClientId]);
		try
		{
			var credential = await dbContext.Credentials.FindAsync(settings.ClientId)
				?? throw new InvalidOperationException("No credential found for supplied client ID.");

			dbContext.Credentials.Remove(credential);
			await dbContext.SaveChangesAsync();
			logger.LogTraceDatabaseOperationSuccessful();
		}
		catch (Exception ex)
		{
			logger.LogCriticalDatabaseOperationFailed(ex);
			AnsiConsole.MarkupLineInterpolated($"[red]Credential '{settings.ClientId}' could not be removed from the configuration. See log for details.[/]");
			return 1;
		}

		AnsiConsole.MarkupLineInterpolated($"[green]Credential '{settings.ClientId}' removed successfully.[/]");
		return 0;
	}
}
