using ACC.Backup.Cli.Data;
using ACC.Backup.Cli.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Rendering;

namespace ACC.Backup.Cli.Commands.Configure.Jobs;

public sealed partial class ListJobCommand(ILogger<ListJobCommand> logger, ConfigurationDbContext dbContext) 
	: AsyncCommand
{
	public override async Task<int> ExecuteAsync(CommandContext context)
	{
		logger.LogInformationCommandStart("ListJobCommand", []);

		var jobTable = new Table();
		jobTable.AddColumn("Job ID");
		jobTable.AddColumn("Item IDs included");
		jobTable.AddColumn("Item IDs excluded");
		jobTable.AddColumn("Storage path");

		var jobs = await dbContext.Jobs.ToListAsync();

		foreach (var job in jobs)
		{
			jobTable.AddRow(
				new Text($"{job.Id}"), 
				new Rows(job.IncludeIds
					.Take(5)
					.Select(x => new Text(x))
					.Concat(job.IncludeIds.Length > 5 ?
						[new Markup($"[grey]{job.IncludeIds.Length - 5} additional inclusions not shown.[/]")] :
						Enumerable.Empty<IRenderable>()
					)
				),
				new Rows(job.IncludeIds
					.Take(5)
					.Select(x => new Text(x))
					.Concat(job.ExcludeIds.Length > 5 ?
						[new Markup($"[grey]{job.ExcludeIds.Length - 5} additional exclusions not shown.[/]")] :
						Enumerable.Empty<IRenderable>()
					)
				),
				new Text(job.StoragePath)
			);
		}

		var credentialPanel = new Panel(
			new Rows(
				new Markup("[bold]Jobs[/]"),
				jobTable
			)
		);

		AnsiConsole.Write(credentialPanel);
		return 0;
	}
}
