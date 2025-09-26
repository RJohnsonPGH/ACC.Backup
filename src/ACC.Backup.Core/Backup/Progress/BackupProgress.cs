namespace ACC.Backup.Core.Backup.Progress;

public sealed record BackupProgress
{
	public required string Id { get; init; }
	public required string Name { get; init; }
	public double PercentComplete { get; set; }
	public long BytesDownloaded { get; set; }
	public long? BytesTotal { get; set; }
	public required BackupStatus Status { get; set; }

	public enum BackupStatus
	{
		InProgress,
		Completed,
		Failed
	}
}
