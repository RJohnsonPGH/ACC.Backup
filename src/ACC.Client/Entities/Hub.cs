namespace ACC.Client.Entities;

/// <summary>
/// A hub containing multiple projects
/// </summary>
public sealed record Hub : IAccApiEntity
{
	public required string Id { get; init; }
	public required string Name { get; init; }
	public required string Region { get; init; }
	public List<Project> Projects { get; init; } = [];
}
