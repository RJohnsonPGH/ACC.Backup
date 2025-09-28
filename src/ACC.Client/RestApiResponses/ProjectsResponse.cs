using System.Text.Json.Serialization;

namespace ACC.Client.RestApiResponses;

/// <summary>
/// Response from the Projects endpoint
/// </summary>
public sealed record ProjectsResponse
{
	[JsonPropertyName("links")]
	[JsonRequired]
	public required ProjectsResponseLinks Links { get; init; }

	[JsonPropertyName("data")]
	[JsonRequired]
	public required List<ProjectResponseProject> Data { get; init; }
}

public sealed record ProjectsResponseLinks
{
	[JsonPropertyName("self")]
	[JsonRequired] 
	public required ProjectsResponseLink Self { get; init; }

	[JsonPropertyName("first")]
	public ProjectsResponseLink? First { get; init; }

	[JsonPropertyName("next")]
	public ProjectsResponseLink? Next { get; init; }
}

public sealed record ProjectsResponseLink
{
	[JsonPropertyName("href")]
	[JsonRequired] 
	public required string Href { get; init; }
}