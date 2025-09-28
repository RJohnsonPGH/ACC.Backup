using System.Text.Json.Serialization;
using ACC.Client.Entities;

namespace ACC.Client.RestApiResponses;

public sealed record FolderContentsResponse
{
	[JsonPropertyName("links")]
	[JsonRequired]
	public required FolderContentsResponseLinks Links { get; init; }

	[JsonPropertyName("data")]
	[JsonRequired]
	public required List<FolderContentsResponseData> Data { get; init; }

	[JsonPropertyName("included")]
	public List<FolderContentsResponseIncluded>? Included { get; init; } = default!;
}

public sealed record FolderContentsResponseLinks
{
	[JsonPropertyName("self")]
	[JsonRequired] 
	public required FolderContentsResponseLink Self { get; init; }

	[JsonPropertyName("first")]
	public FolderContentsResponseLink? First { get; init; } = default!;

	[JsonPropertyName("next")]
	public FolderContentsResponseLink? Next { get; init; } = default!;
}

public sealed record FolderContentsResponseLink
{
	[JsonPropertyName("href")]
	[JsonRequired] 
	public required string Href { get; init; }
}

public sealed record FolderContentsResponseDataAttributesExtensionSchema
{
	[JsonPropertyName("href")]
	[JsonRequired] 
	public required string Href { get; init; }
}

public sealed record FolderContentsResponseData
{
	[JsonPropertyName("type")]
	[JsonRequired] 
	public required AccEntityType Type { get; init; }

	[JsonPropertyName("id")]
	[JsonRequired] 
	public required Urn Id { get; init; }

	[JsonPropertyName("attributes")]
	[JsonRequired] 
	public required FolderContentsResponseDataAttributes Attributes { get; init; }

	[JsonPropertyName("relationships")]
	[JsonRequired] 
	public required FolderContentsResponseRelationships Relationships { get; init; }
}

public sealed record FolderContentsResponseDataAttributesExtension
{
	[JsonPropertyName("type")]
	[JsonRequired] 
	public required string Type { get; init; }

	[JsonPropertyName("version")]
	[JsonRequired] 
	public required string Version { get; init; }

	[JsonPropertyName("schema")]
	[JsonRequired] 
	public required FolderContentsResponseDataAttributesExtensionSchema Schema { get; init; }

	[JsonPropertyName("data")]
	[JsonRequired] 
	public required FolderContentsResponseDataAttributesExtensionData Data { get; init; }
}

public sealed record FolderContentsResponseDataAttributesExtensionData
{
	[JsonPropertyName("sourceFileName")]
	public string? SourceFileName { get; init; } = string.Empty;

	[JsonPropertyName("visibleTypes")]
	public string[] VisibleTypes { get; init; } = [];

	[JsonPropertyName("allowedTypes")]
	public string[] AllowedTypes { get; init; } = [];

	[JsonPropertyName("namingStandardIds")] 
	public string[] NamingStandardIds { get; init; } = [];
}

public sealed record FolderContentsResponseDataAttributes
{
	// This field is intentionally not required, as it does nto exist for all items returned by the API
	[JsonPropertyName("name")]
	public string? Name { get; init; } = string.Empty;

	[JsonPropertyName("displayName")]
	[JsonRequired] 
	public required string DisplayName { get; init; }

	[JsonPropertyName("createTime")]
	[JsonRequired] 
	public required DateTime CreateTime { get; init; }

	[JsonPropertyName("createUserId")]
	[JsonRequired] 
	public required string CreateUserId { get; init; }

	[JsonPropertyName("createUserName")]
	[JsonRequired] 
	public required string CreateUserName { get; init; }

	[JsonPropertyName("lastModifiedTime")]
	[JsonRequired] 
	public required DateTime LastModifiedTime { get; init; }

	[JsonPropertyName("lastModifiedUserId")]
	[JsonRequired] 
	public required string LastModifiedUserId { get; init; }

	[JsonPropertyName("lastModifiedUserName")]
	[JsonRequired] 
	public required string LastModifiedUserName { get; init; }

	[JsonPropertyName("lastModifiedTimeRollup")]
	public DateTime? LastModifiedTimeRollup { get; init; }

	[JsonPropertyName("path")]
	public string? Path { get; init; } = string.Empty;

	[JsonPropertyName("objectCount")]
	public int? ObjectCount { get; init; }

	[JsonPropertyName("hidden")]
	[JsonRequired] 
	public required bool Hidden { get; init; }

	[JsonPropertyName("extension")]
	[JsonRequired] 
	public required FolderContentsResponseDataAttributesExtension Extension { get; init; }
}

public sealed record FolderContentsResponseIncluded
{
	[JsonPropertyName("type")]
	[JsonRequired]
	public required AccEntityType Type { get; init; }

	[JsonPropertyName("id")]
	[JsonRequired]
	public required Urn Id { get; init; }

	[JsonPropertyName("attributes")]
	[JsonRequired]
	public required FolderContentsResponseIncludedAttributes Attributes { get; init; }

	[JsonPropertyName("relationships")]
	[JsonRequired] 
	public required FolderContentsResponseIncludeRelationships Relationships { get; init; }
}

public sealed record FolderContentsResponseIncludedAttributes
{
	[JsonPropertyName("name")]
	[JsonRequired] 
	public required string Name { get; init; }

	[JsonPropertyName("displayName")]
	[JsonRequired] 
	public required string DisplayName { get; init; }

	[JsonPropertyName("createTime")]
	[JsonRequired] 
	public required DateTime CreateTime { get; init; }

	[JsonPropertyName("createUserId")]
	[JsonRequired] 
	public required string CreateUserId { get; init; }

	[JsonPropertyName("createUserName")]
	[JsonRequired] 
	public required string CreateUserName { get; init; }

	[JsonPropertyName("lastModifiedTime")]
	[JsonRequired] 
	public required DateTime LastModifiedTime { get; init; }

	[JsonPropertyName("lastModifiedUserId")]
	[JsonRequired] 
	public required string LastModifiedUserId { get; init; }

	[JsonPropertyName("lastModifiedUserName")]
	[JsonRequired] 
	public required string LastModifiedUserName { get; init; }

	[JsonPropertyName("versionNumber")]
	[JsonRequired] 
	public required int VersionNumber { get; init; }

	[JsonPropertyName("storageSize")]
	public long? StorageSize { get; init; }

	[JsonPropertyName("fileType")]
	public string? FileType { get; init; }

	[JsonPropertyName("hidden")]
	public bool? Hidden { get; init; }

	[JsonPropertyName("reserved")]
	public bool? Reserved { get; init; }

	[JsonPropertyName("reservedTime")]
	public DateTime? ReservedTime { get; init; }

	[JsonPropertyName("reservedUserId")]
	public string? ReservedUserId { get; init; }

	[JsonPropertyName("reservedUserName")]
	public string? ReservedUserName { get; init; }
}

public sealed record FolderContentsResponseIncludeRelationships
{
	[JsonPropertyName("storage")]
	public FolderContentsResponseIncludeRelationshipsStorage? Storage { get; init; } = default!;
}

public sealed record FolderContentsResponseIncludeRelationshipsStorage
{
	[JsonPropertyName("data")]
	[JsonRequired] 
	public required FolderContentsResponseIncludeRelationshipsStorageData Data { get; init; }

	[JsonPropertyName("meta")]
	[JsonRequired] 
	public required FolderContentsResponseIncludeRelationshipsStorageMeta Meta { get; init; }
}

public sealed record FolderContentsResponseIncludeRelationshipsStorageMeta
{
	[JsonPropertyName("link")]
	[JsonRequired] 
	public required FolderContentsResponseLink Link { get; init; }
}

public sealed record FolderContentsResponseIncludeRelationshipsStorageData
{
	[JsonPropertyName("type")]
	[JsonRequired] 
	public required string Type { get; init; }

	[JsonPropertyName("id")]
	[JsonRequired] 
	public required string Id { get; init; }
}

public sealed record FolderContentsResponseRelationships
{
	[JsonPropertyName("parent")]
	[JsonRequired] 
	public required FolderContentsResponseRelationshipsParent Parent { get; init; }
}

public sealed record FolderContentsResponseRelationshipsParent
{
	[JsonPropertyName("data")]
	[JsonRequired] 
	public required FolderContentsResponseRelationshipsParentData Data { get; init; }
}

public sealed record FolderContentsResponseRelationshipsParentData
{
	[JsonPropertyName("type")]
	[JsonRequired] 
	public required AccEntityType Type { get; init; }

	[JsonPropertyName("id")]
	[JsonRequired] 
	public required string Id { get; init; }
}