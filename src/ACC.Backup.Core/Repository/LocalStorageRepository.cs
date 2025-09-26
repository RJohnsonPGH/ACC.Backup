using ACC.Backup.Core.Data;
using ACC.Backup.Core.Download;
using ACC.Backup.Core.Logging;
using ACC.Client.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ACC.Backup.Core.Repository;

/// <summary>
/// A simple local storage repository implementation using SQLite and the local file system.
/// </summary>
/// <param name="logger"></param>
/// <param name="client"></param>
public sealed partial class LocalStorageRepository(ILogger<LocalStorageRepository> logger, ILocalStorageRepositoryPathProvider pathProvider) : IRepository
{
	private readonly Lazy<Task> _initializationTask = new(async () =>
	{
		var databasePath = Path.Combine(pathProvider.RepositoryPath, "repository.db");
		var options = new DbContextOptionsBuilder<RepositoryDbContext>()
			.UseSqlite($"Data Source={databasePath}")
			.Options;

		var context = new RepositoryDbContext(options);
		await context.Database.EnsureCreatedAsync();
	});

	/// <summary>
	/// Creates a new instance of the <see cref="RepositoryDbContext"/> using the current repository path.
	/// </summary>
	/// <returns></returns>
	/// <exception cref="InvalidOperationException">Thrown if the repository has not been initialized with a path</exception>
	private async Task<RepositoryDbContext> CreateRepositoryContextAsync(CancellationToken cancellationToken)
	{
		await _initializationTask.Value;

		var databasePath = Path.Combine(pathProvider.RepositoryPath, "repository.db");
		var options = new DbContextOptionsBuilder<RepositoryDbContext>()
			.UseSqlite($"Data Source={databasePath}")
			.Options;

		return new RepositoryDbContext(options);
	}

public async Task<bool> IngestItemAsync(Item item, string filePath, CancellationToken cancellationToken)
{
	try
	{
		// Combine the root path with the file relative path and then ensure it is created
		var destinationFolder = Path.Combine(pathProvider.RepositoryPath, item.ProjectId, item.Urn.Id, $"{item.Version}");
		var destinationPath = Path.Combine(destinationFolder, item.Name);
		Directory.CreateDirectory(destinationFolder);
		File.Move(filePath, destinationPath, true);

		// Update the file creation and modification times to match the item's last modified time
		File.SetCreationTimeUtc(destinationPath, item.LastModifiedTime);
		File.SetLastWriteTimeUtc(destinationPath, item.LastModifiedTime);

		logger.LogTraceItemIngestSuccessful(item.ProjectId, item.Id, item.Version, destinationPath);

		// Update the database with the new item version
		using var dbContext = await CreateRepositoryContextAsync(cancellationToken);
		var repoItem = await dbContext.Items.FindAsync([item.Id], cancellationToken);

		// If the item does not exist, insert it as a new record
		if (repoItem is null)
		{
			dbContext.Items.Add(new()
			{
				Id = item.Id,
				ProjectId = item.ProjectId,
				ProjectName = item.ProjectName,
				FolderId = item.FolderId,
				Name = item.Name,
				LatestVersion = item.Version
			});
		}
		else
		{
			// While the ID doesnt change, the name or project name might
			repoItem.Name = item.Name;
			repoItem.ProjectName = item.ProjectName;
			repoItem.LatestVersion = item.Version;
		}

		await dbContext.SaveChangesAsync(cancellationToken);
		logger.LogTraceItemMetadataUpdated(item.ProjectId, item.Id, item.Version);

		return true;
	}
	catch (Exception ex)
	{
		File.Delete(filePath);
		logger.LogErrorItemIngestFailed(ex, item.ProjectId, item.Id, item.Version);
		return false;
	}
}

	/// <summary>
	/// Gets the latest version number of the specified item from the repository database.
	/// </summary>
	/// <param name="itemId">The item identifier</param>
	/// <param name="cancellationToken"></param>
	/// <returns>An int of the current version of the item, and 0 if it does not exist.</returns>
	public async Task<int> GetItemVersionFromRepositoryAsync(Item item, CancellationToken cancellationToken)
	{
		using var dbContext = await CreateRepositoryContextAsync(cancellationToken);
		var repoItem = await dbContext.Items.AsNoTracking().FirstOrDefaultAsync(i => i.Id == item.Id, cancellationToken);

		// Item is not in database, all versions should be backed up
		if (repoItem is null)
		{
			logger.LogTraceItemRepositoryVersion(item.ProjectId, item.Id, 0);
			return 0;
		}

		logger.LogTraceItemRepositoryVersion(item.ProjectId, item.Id, repoItem.LatestVersion);
		return repoItem.LatestVersion;
	}

	public async Task<bool> SaveReportToRepositoryAsync(string reportHtml, CancellationToken cancellationToken)
	{
		try
		{
			var reportDirectory = Path.Combine(pathProvider.RepositoryPath, "reports");
			var reportPath = Path.Combine(reportDirectory, $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.html");
			
			Directory.CreateDirectory(reportDirectory);
			await File.WriteAllTextAsync(reportPath, reportHtml, cancellationToken);

			logger.LogInformationReportSaved(reportPath);
			return true;
		}
		catch (Exception ex)
		{
			logger.LogErrorReportSaveFailed(ex);
			return false;
		}
	}
}
