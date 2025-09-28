namespace ACC.Backup.Core.Download;

public interface IDownloadService
{
	Task<bool> DownloadFileAsync(IProgress<DownloadProgress> progress, Uri signedUri, string destinationPath, CancellationToken cancellationToken);
}
