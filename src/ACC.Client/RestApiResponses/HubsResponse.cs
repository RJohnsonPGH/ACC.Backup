using System.Text.Json.Serialization;

namespace ACC.Client.RestApiResponses;

/// <summary>
/// Response from the Hubs endpoint
/// </summary>
public sealed record HubsResponse
{
	[JsonPropertyName("links")]
	[JsonRequired]
	public required HubsResponseLinks Links { get; init; }

	[JsonPropertyName("data")]
	[JsonRequired]
	public required List<HubsResponseData> Data { get; init; }
}

public record HubsResponseData
{
	[JsonPropertyName("type")]
	[JsonRequired]
	public required string Type { get; init; }

	[JsonPropertyName("id")]
	[JsonRequired]
	public required string Id { get; init; }

	[JsonPropertyName("attributes")]
	[JsonRequired]
	public required HubsResponseDataAttributes Attributes { get; init; }

	[JsonPropertyName("links")]
	[JsonRequired]
	public required HubsResponseLinks Links { get; init; }
}

public record HubsResponseDataAttributes
{
	[JsonPropertyName("name")]
	[JsonRequired]
	public required string Name { get; init; }

	[JsonPropertyName("extension")]
	[JsonRequired]
	public required HubsResponseDataAttributesExtension Extension { get; init; }

	[JsonPropertyName("region")]
	[JsonRequired]
	public required string Region { get; init; }
}

public record HubsResponseDataAttributesExtension
{
	[JsonPropertyName("type")]
	[JsonRequired]
	public required string Type { get; init; }

	[JsonPropertyName("version")]
	[JsonRequired]
	public required string Version { get; init; }

	[JsonPropertyName("schema")]
	[JsonRequired]
	public required HubsResponseLink Schema { get; init; }
}

public record HubsResponseLinks
{
	[JsonPropertyName("self")]
	[JsonRequired]
	public required HubsResponseLink Self { get; init; }

	[JsonPropertyName("related")]
	public HubsResponseLink? Related { get; init; }
}

public record HubsResponseLink
{
	[JsonPropertyName("href")]
	[JsonRequired]
	public required string Href { get; init; }
}
