namespace ACC.Backup.Core.Data;

public sealed record RepositoryItem
{
	public required string Id { get; init; }
	public required string Name { get; set; }
	public required string ProjectId { get; set; }
	public required string ProjectName { get; set; }
	public required string FolderId { get; set; }
	public required int LatestVersion { get; set; }
}
