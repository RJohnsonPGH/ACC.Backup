using System.Text.Json.Serialization;

namespace ACC.Client.Entities;

/// <summary>
/// The type of an ACC entity
/// </summary>
public enum AccEntityType
{
	[JsonPropertyName("hubs")]
	Hubs,
	[JsonPropertyName("projects")]
	Projects,
	[JsonPropertyName("folders")]
	Folders,
	[JsonPropertyName("lineage")]
	Lineage,
	[JsonPropertyName("versions")]
	Versions,
	[JsonPropertyName("items")]
	Items
}
