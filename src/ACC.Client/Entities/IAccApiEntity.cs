namespace ACC.Client.Entities;

/// <summary>
/// A common interface for ACC API entities
/// </summary>
public interface IAccApiEntity
{
	string Id { get; }
	string Name { get; }
}
