using ACC.Backup.Core.Repository;
using ACC.Client.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ACC.Backup.Core.Test.Repository;

public sealed class LocalStorageRepositoryTests(ITestOutputHelper outputHelper)
{
	[Fact]
	public async Task BackupItemToRepositoryAsync_DownloadsFile_Successfully()
	{
		// Arrange
		var item = new Item
		{
			Id = "urn:adsk.wipprod:fs.file:vf.item1?version=2",
			Urn = "urn:adsk.wipprod:fs.file:vf.item1?version=2",
			ProjectId = "proj1",
			ProjectName = "Test Project",
			FolderId = "folder",
			Name = "My File",
			Version = 2,
			CreateTime = DateTime.UtcNow.AddDays(-1),
			LastModifiedTime = DateTime.UtcNow.AddDays(-1),
		};

		// File system
		var tempDir = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}");
		Directory.CreateDirectory(tempDir);
		var pathProvider = Substitute.For<ILocalStorageRepositoryPathProvider>();
		pathProvider.RepositoryPath.Returns(tempDir);

		var testFile = Path.GetTempFileName();
		File.SetLastWriteTimeUtc(testFile, DateTime.UtcNow);

		// Set up DI
		using var services = new ServiceCollection()
			.AddLogging(builder =>
			{
				builder.AddXUnit(outputHelper);
				builder.SetMinimumLevel(LogLevel.Trace);
			})

			.AddSingleton(pathProvider)
			.AddSingleton<IRepository, LocalStorageRepository>()
			.BuildServiceProvider();
		var repository = services.GetRequiredService<IRepository>();

		// Act
		var result = await repository.IngestItemAsync(item, testFile, TestContext.Current.CancellationToken);

		var repositoryItemVersion = await repository.GetItemVersionFromRepositoryAsync(item, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result);

		// Verify file exists
		var expectedFilePath = Path.Combine(tempDir, "proj1", "vf.item1", "2", "file.dat");
		Assert.True(File.Exists(expectedFilePath));

		// Verify file creation date
		var fileInfo = new FileInfo(expectedFilePath);
		Assert.Equal(item.CreateTime, fileInfo.CreationTimeUtc, TimeSpan.FromSeconds(1));
		Assert.Equal(item.LastModifiedTime, fileInfo.LastWriteTimeUtc, TimeSpan.FromSeconds(1));

		// Verify database file version
		Assert.Equal(item.Version, repositoryItemVersion);

		// Clean up
		//await services.DisposeAsync();
		//await Task.Delay(1000, TestContext.Current.CancellationToken);
		//Directory.Delete(tempDir, true);
	}
}
