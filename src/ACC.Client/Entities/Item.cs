namespace ACC.Client.Entities;

/// <summary>
/// An item (file) within a folder
/// </summary>
public sealed class Item
{
	public required string Id { get; init; }
	public required Urn Urn { get; init; }
	public required string Name { get; init; }
	public required string ProjectId { get; init; }
	public required string ProjectName { get; init; }
	public required string FolderId { get; init; }
	public required DateTime CreateTime { get; init; }
	public required DateTime LastModifiedTime { get; init; }
	public required int Version { get; init; }
	public string? DownloadUrl { get; init; }
}
