namespace ACC.Client.Entities;

/// <summary>
/// A project within a hub
/// </summary>
public sealed class Project : IAccApiEntity
{
	public required string Id { get; init; }
	public required string Name { get; init; }
	public required string RootFolderId { get; init; }
}
