using System.ComponentModel;
using ACC.Backup.Cli.Data;
using ACC.Backup.Cli.Logging;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ACC.Backup.Cli.Commands.Configure.Credentials;

public sealed partial class AddCredentialCommand(ILogger<AddCredentialCommand> logger, ConfigurationDbContext dbContext) 
	: AsyncCommand<AddCredentialCommand.Settings>
{
	public sealed class Settings : CommandSettings
	{
		[CommandOption("-c|--clientId", true)]
		[Description("The client id of the credential to add to the configuration.")]
		public required string ClientId { get; init; }

		[CommandOption("-s|--clientSecret", true)]
		[Description("The client secret of the credential to add to the configuration.")]
		public required string ClientSecret { get; init; }
	}

	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
	{
		logger.LogInformationCommandStart("AddCredentialCommand", [settings.ClientId, settings.ClientSecret[^4..]]);

		var newCredential = new Credential()
		{
			Id = settings.ClientId,
			Secret = settings.ClientSecret,
		};
		try
		{
			await dbContext.Credentials.AddAsync(newCredential);
			await dbContext.SaveChangesAsync();
			logger.LogTraceDatabaseOperationSuccessful();
		}
		catch (Exception ex)
		{
			logger.LogCriticalDatabaseOperationFailed(ex);
			AnsiConsole.MarkupLineInterpolated($"[red]Credential '{newCredential.Id}' could not be added to the configuration. See log for details.[/]");
			return 1;
		}

		AnsiConsole.MarkupLineInterpolated($"[green]Credential '{newCredential.Id}' added successfully.[/]");
		return 0;
	}
}
