namespace ACC.Backup.Core.Exclusion;

public interface IExclusionService
{
	bool ShouldExcludeItem(string id);
}
