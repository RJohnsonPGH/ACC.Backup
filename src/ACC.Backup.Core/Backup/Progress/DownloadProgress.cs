namespace ACC.Backup.Core.Backup.Progress;

public sealed record DownloadProgress
{
	public required string Id { get; init; }
	public required string Name { get; init; }
	public double PercentComplete { get; set; }
	public long BytesDownloaded { get; set; }
	public long? BytesTotal { get; set; }
	public required DownloadStatus Status { get; set; }

	public enum DownloadStatus
	{
		InProgress,
		Completed,
		Failed
	}
}
