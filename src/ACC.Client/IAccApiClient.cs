using ACC.Client.Entities;

namespace ACC.Client;

/// <summary>
/// Client interface for interacting with the ACC API.
/// </summary>
public interface IAccApiClient
{
	public IAsyncEnumerable<Hub> GetHubsAsync(CancellationToken cancellationToken = default);
	public IAsyncEnumerable<Project> GetProjectsAsync(Hub hub, CancellationToken cancellationToken = default);
	public IAsyncEnumerable<Item> GetProjectContentsAsync(Project project, CancellationToken cancellationToken = default);
	public Task<Uri> GetSignedDownloadUriAsync(Uri downloadUri, CancellationToken cancellationToken = default);
}