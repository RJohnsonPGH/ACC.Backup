namespace ACC.Backup.Cli.Data;

/// <summary>
/// A job definition for backing up and restoring assets.
/// </summary>
public sealed record Job
{
	public int Id { get; set; }
	public required string[] IncludeIds { get; set; } = [];
	public required string[] ExcludeIds { get; set; } = [];
	public required string StoragePath { get; set; }
}
