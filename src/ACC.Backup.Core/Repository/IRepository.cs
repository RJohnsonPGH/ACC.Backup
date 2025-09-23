using ACC.Client.Entities;

namespace ACC.Backup.Core.Repository;

public interface IRepository
{
	/// <summary>
	/// Gets the version number of the specified item from the repository.
	/// </summary>
	/// <param name="itemId"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task<int> GetItemVersionFromRepositoryAsync(Item item, CancellationToken cancellationToken = default);

	/// <summary>
	/// Downloads the specified item from the provided S3 download URI, reporting progress to the provided IProgress instance.
	/// </summary>
	/// <param name="progress"></param>"
	/// <param name="item"></param>
	/// <param name="signedUri"></param>
	/// <param name="cancellationToken"></param>
	/// <returns>True if the backup was succesful, false if it failed.</returns>
	Task<bool> BackupItemToRepositoryAsync(IProgress<double> progress, Item item, Uri signedUri, CancellationToken cancellationToken = default);

	/// <summary>
	/// Saves the provided HTML report to the repository.
	/// </summary>
	/// <param name="reportHtml"></param>
	/// <param name="cancellationToken"></param>
	/// <returns>True if the report was successfully saved, false if it failed to save.</returns>
	Task<bool> SaveReportToRepositoryAsync(string reportHtml, CancellationToken cancellationToken = default);
}
