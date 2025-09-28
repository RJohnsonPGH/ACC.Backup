using ACC.Backup.Core.Download;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ACC.Backup.Core.Test.Download;

public sealed class BasicDownloadServiceTests(ITestOutputHelper outputHelper)
{
	[Fact]
	public async Task DownloadFileAsync_ShouldReturnTrue_WhenSuccessful()
	{
		// Arrange
		// Set up DI
		using var services = new ServiceCollection()
			.AddLogging(builder =>
			{
				builder.AddXUnit(outputHelper);
				builder.SetMinimumLevel(LogLevel.Trace);
			})
			.AddHttpClient()
			.AddSingleton<IDownloadService, BasicDownloadService>()
			.BuildServiceProvider();
		var downloadService = services.GetRequiredService<IDownloadService>();

		// Act
		var path = Path.GetTempFileName();
		var result = await downloadService.DownloadFileAsync(
			new Progress<DownloadProgress>(),
			new Uri("http://localhost/test.bin"),
			path,
			CancellationToken.None
		);

		// Assert
		Assert.True(result);
		File.Delete(path);
	}
}
