using ACC.Backup.Cli.Commands.Backup;
using ACC.Backup.Cli.Providers;
using Spectre.Console.Cli;

namespace ACC.Backup.Cli.Interceptors;

/// <summary>
/// An interceptor that sets the JobId in the JobIdProvider based on the command settings.
/// </summary>
/// <param name="jobIdProvider">The JobIdProvider that is shared across dependent services.</param>
public sealed class JobIdProviderInterceptor(JobIdProvider jobIdProvider) : ICommandInterceptor
{
	/// <summary>
	/// Intercepts the command execution to set the JobId in the JobIdProvider.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="settings"></param>
	public void Intercept(CommandContext context, CommandSettings settings)
	{
		// Setting the JobId is only valid for BackupCommand
		if (settings is not BackupCommand.Settings backupSettings)
		{
			return;
		}

		jobIdProvider.JobId = backupSettings.JobId;
	}
}
