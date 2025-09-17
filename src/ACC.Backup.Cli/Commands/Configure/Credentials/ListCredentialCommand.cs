using ACC.Backup.Cli.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using ACC.Backup.Cli.Logging;

namespace ACC.Backup.Cli.Commands.Configure.Credentials;

public sealed partial class ListCredentialCommand(ILogger<ListCredentialCommand> logger, ConfigurationDbContext dbContext) 
	: AsyncCommand
{
	public override async Task<int> ExecuteAsync(CommandContext context)
	{
		logger.LogInformationCommandStart("ListCredentialCommand", []);
		var credentialTable = new Table();
		credentialTable.AddColumn("Client ID");
		credentialTable.AddColumn("Client Secret");

		var credentials = await dbContext.Credentials.ToListAsync();

		foreach (var credential in credentials)
		{
			credentialTable.AddRow(credential.Id, credential.Secret);
		}

		var credentialPanel = new Panel(
			new Rows(
				new Markup("[bold]Credentials[/]"),
				credentialTable
			)
		);

		AnsiConsole.Write(credentialPanel);
		return 0;
	}
}
