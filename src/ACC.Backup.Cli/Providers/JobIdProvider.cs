namespace ACC.Backup.Cli.Providers;

public sealed record JobIdProvider
{
	public int JobId { get; internal set; }
}
