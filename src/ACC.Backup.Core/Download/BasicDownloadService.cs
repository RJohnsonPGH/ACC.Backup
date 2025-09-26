using ACC.Backup.Core.Logging;
using Microsoft.Extensions.Logging;

namespace ACC.Backup.Core.Download;

public sealed class BasicDownloadService(ILogger<BasicDownloadService> logger, HttpClient client) : IDownloadService
{
	public async Task<bool> DownloadFileAsync(IProgress<DownloadProgress> progress, Uri signedUri, string destinationPath, CancellationToken cancellationToken)
	{
		try
		{
			// Perform the HTTP request and verify the response indicates success
			var response = await client.GetAsync(signedUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
			if (!response.IsSuccessStatusCode)
			{
				logger.LogErrorHttpRequestNotSuccesful(signedUri, response.StatusCode);
				return false;
			}

			// Get the total file size from the Content-Length header if available
			var totalBytes = response.Content.Headers.ContentLength;
			logger.LogTraceDownloadFileSize(signedUri, totalBytes ?? 0);

			// Download the file in chunks, reporting progress
			var options = new FileStreamOptions()
			{
				Mode = FileMode.Create,
				Access = FileAccess.Write,
				Share = FileShare.None,
				BufferSize = 131072,
				Options = FileOptions.Asynchronous | FileOptions.SequentialScan
			};
			await using var fileStream = File.Open(destinationPath, options);
			await using var httpStream = await response.Content.ReadAsStreamAsync(cancellationToken);

			var buffer = new byte[131072];
			long totalRead = 0;
			int read;
			while ((read = await httpStream.ReadAsync(buffer, cancellationToken)) > 0)
			{
				cancellationToken.ThrowIfCancellationRequested();

				await fileStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
				totalRead += read;
				progress.Report(new(totalRead, totalBytes ?? 0, 0));
			}

			return true;
		}
		catch(Exception ex)
		{
			logger.LogErrorDownloadFileFailed(ex, signedUri);
			return false;
		}
	}
}
