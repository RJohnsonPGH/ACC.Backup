using System.IO.Pipelines;
using ACC.Backup.Core.Logging;
using Microsoft.Extensions.Logging;

namespace ACC.Backup.Core.Download;

public sealed class BasicDownloadService(ILogger<BasicDownloadService> logger, HttpClient client) : IDownloadService
{
	private readonly FileStreamOptions _options = new()
	{
		Mode = FileMode.Create,
		Access = FileAccess.Write,
		Share = FileShare.None,
		BufferSize = 1048576,
		Options = FileOptions.Asynchronous | FileOptions.SequentialScan
	};

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
			var pipe = new Pipe();
			await using var fileStream = File.Open(destinationPath, _options);
			await using var httpStream = await response.Content.ReadAsStreamAsync(cancellationToken);

			var readTask = Task.Run(async () =>
			{
				const int minBufferSize = 1048576;
				try
				{
					while (true)
					{
						Memory<byte> memory = pipe.Writer.GetMemory(minBufferSize);
						int bytesRead = await httpStream.ReadAsync(memory, cancellationToken);
						if (bytesRead == 0)
						{
							break;
						}

						pipe.Writer.Advance(bytesRead);

						var result = await pipe.Writer.FlushAsync(cancellationToken);
						if (result.IsCompleted)
						{
							break;
						}
					}
				}
				catch (Exception ex)
				{
					pipe.Writer.Complete(ex);
					return;
				}

				await pipe.Writer.CompleteAsync();
			}, cancellationToken);

			var writeTask = Task.Run(async () =>
			{
				long totalRead = 0;
				try
				{
					while (true)
					{
						var result = await pipe.Reader.ReadAsync(cancellationToken);
						var sequence = result.Buffer;

						foreach (var segment in sequence)
						{
							await fileStream.WriteAsync(segment, cancellationToken);
							totalRead += segment.Length;
						}

						progress.Report(new DownloadProgress(totalRead, totalBytes ?? 0, 0));
						pipe.Reader.AdvanceTo(sequence.End);

						if (result.IsCompleted)
						{
							break;
						}
					}
				}
				catch (Exception ex)
				{
					pipe.Reader.Complete(ex);
					return;
				}

				await pipe.Reader.CompleteAsync();
			}, cancellationToken);

			await Task.WhenAll(readTask, writeTask);
			return true;
		}
		catch(Exception ex)
		{
			logger.LogErrorDownloadFileFailed(ex, signedUri);
			return false;
		}
	}
}
