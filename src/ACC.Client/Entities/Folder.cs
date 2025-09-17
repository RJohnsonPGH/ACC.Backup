namespace ACC.Client.Entities;

/// <summary>
/// A folder within a project
/// </summary>
public sealed class Folder
{
	public required string Id { get; init; }
	public required string Name { get; init; }
	public List<Folder> Subfolders { get; init; } = [];
	public IEnumerable<Item> Files { get; init; } = [];
}
