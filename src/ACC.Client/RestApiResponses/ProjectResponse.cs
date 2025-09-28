using System.Text.Json.Serialization;

namespace ACC.Client.RestApiResponses;

public sealed record ProjectResponse
{
	[JsonPropertyName("data")]
	[JsonRequired] 
	public required ProjectResponseProject Data { get; init; }
}

public sealed record ProjectResponseProject
{
	[JsonPropertyName("type")]
	[JsonRequired] 
	public required string Type { get; init; }

	[JsonPropertyName("id")]
	[JsonRequired] 
	public required string Id { get; init; }

	[JsonPropertyName("attributes")]
	[JsonRequired] 
	public required ProjectResponseDataProjectAttributes Attributes { get; init; }

	[JsonPropertyName("relationships")]
	[JsonRequired] 
	public required ProjectResponseDataProjectRelationships Relationships { get; init; }
}

public sealed record ProjectResponseDataProjectAttributes
{
	[JsonPropertyName("name")]
	[JsonRequired] 
	public required string Name { get; init; }
}

public sealed record ProjectResponseDataProjectRelationships
{
	[JsonPropertyName("rootFolder")]
	[JsonRequired] 
	public required ProjectResponseDataProjectRelationshipsRootFolder RootFolder { get; init; }
}

public sealed record ProjectResponseDataProjectRelationshipsRootFolder
{
	[JsonPropertyName("data")]
	[JsonRequired] 
	public required ProjectResponseDataProjectRelationshipsRootFolderData Data { get; init; }
}

public sealed record ProjectResponseDataProjectRelationshipsRootFolderData
{
	[JsonPropertyName("id")]
	[JsonRequired] 
	public required string Id { get; init; }
}