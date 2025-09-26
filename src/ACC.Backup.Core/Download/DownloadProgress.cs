namespace ACC.Backup.Core.Download;

/// <summary>
/// Progress information for a download operation.
/// </summary>
/// <param name="BytesDownloaded">The total number of bytes downloaded.</param>
/// <param name="BytesTotal">The total size of the file as reported by the server, or 0 if the file size was not specified.</param>
/// <param name="DownloadRate">The rolling average download rate of the file, in bytes per second.</param>
public sealed record DownloadProgress(long BytesDownloaded, long BytesTotal, long DownloadRate);
