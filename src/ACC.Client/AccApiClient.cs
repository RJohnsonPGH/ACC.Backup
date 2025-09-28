using Microsoft.Extensions.Logging;
using Flurl;
using System.Runtime.CompilerServices;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ACC.Client.Logging;
using ACC.Client.RestApiResponses;
using ACC.Client.Entities;

namespace ACC.Client;

public sealed partial class AccApiClient(ILogger<AccApiClient> logger, HttpClient client) : IAccApiClient
{
	private readonly static Uri ProjectHubUri = new("https://developer.api.autodesk.com/project/v1/hubs/");
	private readonly static Uri DataProjectUri = new("https://developer.api.autodesk.com/data/v1/projects/");
	private readonly static Uri SignedDownloadUri = new("https://developer.api.autodesk.com/oss/v2/buckets/");

	/// <summary>
	/// Retrieves all hubs accessible with the current authentication context.
	/// </summary>
	/// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
	/// <returns></returns>
	public async IAsyncEnumerable<Hub> GetHubsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var requestUri = ProjectHubUri;
		
		// API docs make no mention of pagination for Hubs. For now, we assume that all hubs will be returned with one call
		// We use IAsyncEnumerable for the sake of consistency with the rest of the API client
		var result = await GetFromApiAsync<HubsResponse>(requestUri, cancellationToken);

		foreach (var hub in result.Data)
		{
			logger.LogDebugRetrievedItem([hub.Id, hub.Attributes.Name]);
			yield return new()
			{
				Id = hub.Id,
				Name = hub.Attributes.Name,
				Region = hub.Attributes.Region,
			};
		}
	}

	/// <summary>
	/// Retrieves all projects within a specified hub.
	/// </summary>
	/// <param name="hub"></param>
	/// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
	/// <returns></returns>
	public async IAsyncEnumerable<Project> GetProjectsAsync(Hub hub, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		// Construct the initial request URI for retrieving projects within the specified hub
		logger.LogInformationRetrieveChildren([hub.Id]);
		var requestUri = ProjectHubUri
			.AppendPathSegments(hub.Id, "projects")
			.ToUri();

		// The API uses pagination, so we need to loop until there are no more pages
		while (requestUri is not null)
		{
			// Retrieve the current page of projects from the API
			var response = await GetFromApiAsync<ProjectsResponse>(requestUri, cancellationToken);
			foreach (var project in response.Data)
			{
				// Log and return each project
				logger.LogDebugRetrievedItem([project.Id, project.Attributes.Name]);
				yield return new()
				{
					Id = project.Id,
					Name = project.Attributes.Name,
					RootFolderId = project.Relationships.RootFolder.Data.Id,
				};
			}
			logger.LogInformationRetrievedItemCount([hub.Id], response.Data.Count);

			// Check if there is a next page
			requestUri = response.Links.Next?.Href is not null ? new Uri(response.Links.Next.Href) : null;
		}
	}

	/// <summary>
	/// Retrieves all items within a specified project, including items in subfolders.
	/// </summary>
	/// <param name="project"></param>
	/// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
	/// <returns></returns>
	public async IAsyncEnumerable<Item> GetProjectContentsAsync(Project project, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		logger.LogInformationRetrieveChildren([project.Id]);
		await foreach (var item in GetFolderContentsInternalAsync(project, project.RootFolderId, cancellationToken))
		{
			yield return item;
		}
	}

	/// <summary>
	/// Recursively retrieves all items within a specified folder, including items in subfolders.
	/// </summary>
	/// <param name="project"></param>
	/// <param name="folderId"></param>
	/// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
	/// <returns></returns>
	private async IAsyncEnumerable<Item> GetFolderContentsInternalAsync(Project project, string folderId, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		// Construct the initial request URI for retrieving folder contents
		logger.LogInformationRetrieveChildren([project.Id, folderId]);
		var requestUri = DataProjectUri
			.AppendPathSegments(project.Id, "folders", folderId, "contents")
			.ToUri();

		// The API uses pagination, so we need to loop until there are no more pages
		while (requestUri is not null)
		{
			var response = await GetFromApiAsync<FolderContentsResponse>(requestUri, cancellationToken);

			// The `Data` property should only contain Type `folders`, but it does occasionally contain `items` indicating a 'lineage' URN
			// Filter these out as attempting to request contents on `items` will result in a Bad Request.
			foreach (var subfolder in response.Data.Where(x => x.Type is AccEntityType.Folders))
			{
				logger.LogDebugRetrievedItem([subfolder.Type.ToString(), subfolder.Id, subfolder.Attributes.Name ?? string.Empty]);

				// Subfolders with zero ObjectCount can be skipped as they contain no items
				if (subfolder.Attributes.ObjectCount == 0)
				{
					logger.LogInformationSkipZeroObjectCountItem([subfolder.Type.ToString(), subfolder.Id, subfolder.Attributes.Name ?? string.Empty]);
					continue;
				}

				// Recursively retrieve contents of the subfolder
				await foreach (var item in GetFolderContentsInternalAsync(project, subfolder.Id, cancellationToken))
				{
					yield return item;
				}
			}

			// The `Included` property contains the actual items (files) within the folder
			foreach (var item in response.Included ?? [])
			{
				// Log and return each item
				logger.LogDebugRetrievedItem([item.Type.ToString(), item.Id, item.Attributes.Name]);
				yield return new()
				{
					Id = item.Id,
					Urn = item.Id,
					ProjectId = project.Id,
					ProjectName = project.Name,
					FolderId = folderId,
					Name = item.Attributes.Name,
					CreateTime = item.Attributes.CreateTime,
					LastModifiedTime = item.Attributes.LastModifiedTime,
					Version = item.Attributes.VersionNumber,
					DownloadUrl = item.Relationships.Storage?.Meta.Link.Href
				};
			}
			logger.LogInformationRetrievedItemCount([project.Id, folderId], response.Data.Count);

			// Check if there is a next page
			requestUri = response.Links.Next?.Href is not null ? new Uri(response.Links.Next.Href) : null;
		}
	}

	/// <summary>
	/// Generates a signed S3 download URL for a given download URI.
	/// </summary>
	/// <param name="downloadUri"></param>
	/// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
	/// <returns></returns>
	public async Task<Uri> GetSignedDownloadUriAsync(Uri downloadUri, CancellationToken cancellationToken = default)
	{
		// Update indices to correctly extract bucket and object keys
		// The URL format is typically: https://developer.api.autodesk.com/oss/v2/buckets/wip.dm.prod/objects/object-id.rvt
		// When split, the segments are: ["/", "oss", "v2", "buckets", "wip.dm.prod", "objects", "object-id.rvt"]
		var bucketId = downloadUri.Segments[4];
		var objectId = downloadUri.Segments[6];

		var requestUri = SignedDownloadUri
			.AppendPathSegments(bucketId, "objects", objectId, "signeds3download")
			//.AppendQueryParam("minutesExpiration", 30)
			.ToUri();

		var result = await GetFromApiAsync<SignedUrlResponse>(requestUri, cancellationToken);
		logger.LogDebugRetrievedItem([result.Url]);

		return new(result.Url);
	}

	/// <summary>
	/// Generic method to perform GET requests to the ACC API and deserialize the response.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="requestUri"></param>
	/// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
	/// <returns></returns>
	/// <exception cref="HttpRequestException"></exception>
	/// <exception cref="InvalidOperationException"></exception>
	private async Task<T> GetFromApiAsync<T>(Uri requestUri, CancellationToken cancellationToken = default)
	{
		var response = await client.GetAsync(requestUri, cancellationToken);

		// Check if the response indicates success and handle errors appropriately
		if (!response.IsSuccessStatusCode)
		{
			var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
			logger.LogErrorHttpRequestNotSuccesful(typeof(T).Name, response.StatusCode.ToString(), errorContent);
			throw new HttpRequestException($"Failed to retrieve data from {requestUri}. Status: {response.StatusCode}. Response: {errorContent}");
		}

		// Configure JSON deserialization options
		var options = new JsonSerializerOptions();
		options.Converters.Add(new JsonStringEnumConverter());
		options.Converters.Add(new UrnJsonConverter());

		// Deserialize the response content into the specified type
		var result = await response.Content.ReadFromJsonAsync<T>(options, cancellationToken) ??
			throw new InvalidOperationException($"Deserialization of {typeof(T).Name} failed.");

		return result;
	}
}