using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;
using ACC.Backup.Cli.Internal;
using ACC.Backup.Cli.Commands.Configure.Credentials;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using ACC.Backup.Cli.Commands.Configure.Jobs;
using ACC.Backup.Core.Repository;
using ACC.Backup.Cli.Commands.Backup;
using ACC.Backup.Core.Backup;
using ACC.Backup.Core.Exclusion;
using ACC.Backup.Cli.Data;
using NReco.Logging.File;
using Microsoft.Extensions.Logging;
using ACC.Client;
using ACC.Client.Authentication.Tokens;
using ACC.Backup.Cli.Providers;
using ACC.Backup.Cli.Interceptors;
using ACC.Backup.Core.Reporting;
using ACC.Backup.Cli.Logging;
using ACC.Backup.Core.Download;

var services = new ServiceCollection();

//services.Configure<EnvironmentVariableOptions>(options =>
//{
//	options.DatabasePath = Environment.GetEnvironmentVariable("DATABASE_PATH");
//	options.ReportPath = Environment.GetEnvironmentVariable("REPORT_PATH");
//	options.LogPath = Environment.GetEnvironmentVariable("LOG_PATH");
//});

services.AddDbContext<ConfigurationDbContext>((serviceProvider, options) =>
{
	var dbOptions = serviceProvider.GetRequiredService<IOptions<EnvironmentVariableOptions>>().Value;

	var dbPath = string.IsNullOrWhiteSpace(dbOptions.DatabasePath)
		? Path.Combine(AppContext.BaseDirectory, "credentials.db")
		: dbOptions.DatabasePath;
	
	options.UseSqlite($"Data Source={dbPath}");
});

// Configure client
services.AddAccApiClient(configure =>
{
	configure.WithTokenService<TokenService>();
	configure.WithTokenCredentialProvider<DatabaseCredentialProvider>();
});

// Configure backup service
var jobIdProvider = new JobIdProvider();
services.AddSingleton(jobIdProvider);

// Merge this into AddBackupService?
services.AddSingleton<ILocalStorageRepositoryPathProvider, DatabaseLocalStorageRepositoryPathProvider>();
services.AddSingleton<IExclusionProvider, DatabaseExclusionProvider>();
services.AddSingleton<IDegreeOfParallelismProvider, DatabaseDegreeOfParallelismProvider>();

services.AddBackupService(configure =>
{
	configure.WithExclusionService<BasicExclusionService>();
	configure.WithDownloadService<BasicDownloadService>();
	configure.WithRepositoryService<LocalStorageRepository>();
	configure.WithReportingService<BasicReportingService>();
});

var registrar = new TypeRegistrar(services);

// Build the database
var serviceProvider = services.BuildServiceProvider();
using (var scope = serviceProvider.CreateScope())
{
	var dbContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
	dbContext.Database.EnsureCreated();
}

// Configure logging
// Because we are building the service provider above to create the database, we need to add logging after that.
// This prevents multiple attempts at opening the log file, which will fail.
services.AddLogging(builder =>
{
	builder
		.EnableRedaction();
	builder
		.AddFilter("Microsoft", LogLevel.Warning)
		.AddFilter("System", LogLevel.Warning)
		.AddFilter("ACC.Backup", LogLevel.Information);
	builder.AddFile("log.txt");
});

services.AddRedaction(builder =>
{
	builder.SetRedactor<HashRedactor>(SecurityClassifications.Token);
});

// Configure and run the application
var interceptor = new JobIdProviderInterceptor(jobIdProvider);

var app = new CommandApp(registrar);

app.Configure(config =>
{
	config.Settings.ApplicationName = "ACC.Backup.Cli";

	config.SetInterceptor(interceptor);

	config.PropagateExceptions();

	config.AddBranch("credential", command =>
	{
		command.SetDescription("Manage API credentials. Multiple sets of credentials can be used for load balancing, provided they all have identical access.");

		command.AddCommand<AddCredentialCommand>("add")
			.WithDescription("Add API credentials.")
			.WithExample("credential add <CLIENTID> <CLIENTSECRET>");

		command.AddCommand<RemoveCredentialCommand>("remove")
			.WithDescription("Remove API credentials.")
			.WithExample("credential remove <CLIENTID>");

		command.AddCommand<ListCredentialCommand>("list")
			.WithDescription("List saved credentials.")
			.WithExample("credential list");
	});

	config.AddBranch("job", command =>
	{
		command.SetDescription("Manage backup jobs.");

		command.AddCommand<AddJobCommand>(@"add --path <REPOSITORYPATH>")
			.WithDescription("Add backup job.")
			.WithExample("job add");

		command.AddCommand<RemoveJobCommand>("remove")
			.WithDescription("Remove backup job.")
			.WithExample("job remove <JOBID>");

		command.AddCommand<ListJobCommand>("list")
			.WithDescription("List backup jobs.")
			.WithExample("job list");
	});

	config.AddCommand<BackupCommand>("backup")
		.WithDescription("Execute ACC backup.")
		.WithExample("backup --job <JOBID>");
});

await app.RunAsync(args);
return 0;
