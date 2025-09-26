using ACC.Backup.Core.Download;
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
	/// Ingests the specified item into the repository from the provided file path.
	/// </summary>
	/// <param name="item"></param>
	/// <param name="filePath"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task<bool> IngestItemAsync(Item item, string filePath, CancellationToken cancellationToken = default);

	/// <summary>
	/// Saves the provided HTML report to the repository.
	/// </summary>
	/// <param name="reportHtml"></param>
	/// <param name="cancellationToken"></param>
	/// <returns>True if the report was successfully saved, false if it failed to save.</returns>
	Task<bool> SaveReportToRepositoryAsync(string reportHtml, CancellationToken cancellationToken = default);
}
