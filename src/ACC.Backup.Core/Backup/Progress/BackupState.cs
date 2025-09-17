namespace ACC.Backup.Core.Backup.Progress;

public enum BackupState
{
	FailedNoDownloadUri,
	FailedDownload,
	SuccessNoDownloadNeeded,
	SuccessDownloaded,
}
