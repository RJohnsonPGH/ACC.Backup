namespace ACC.Backup.Core.Exclusion;

public interface IExclusionProvider
{
	string[] IncludedIds { get; }
	string[] ExcludedIds { get; }
}
