using ACC.Backup.Core.Logging;
using Microsoft.Extensions.Logging;

namespace ACC.Backup.Core.Exclusion;

public partial class BasicExclusionService(ILogger<BasicExclusionService> logger, IExclusionProvider exclusionProvider) : IExclusionService
{
	public bool ShouldExcludeItem(string id)
	{
		// No inclusions or exclusions are set, no items should be excluded
		if (exclusionProvider.IncludedIds.Length == 0 && exclusionProvider.ExcludedIds.Length == 0)
		{
			return false;
		}

		// If the ID is specifically excluded, it should not be included - even if it is also in the inclusion list
		if (exclusionProvider.ExcludedIds.Contains(id))
		{
			logger.LogDebugIdExcluded(id);
			return true;
		}

		// If there are no inclusions, and the ID is not excluded, it should not be excluded
		if (exclusionProvider.IncludedIds.Length == 0)
		{
			return false;
		}

		// If there are inclusions, and the ID is in the inclusion list, it should not be excluded
		if (exclusionProvider.IncludedIds.Contains(id))
		{
			logger.LogDebugIdIncluded(id);
			return false;
		}

		// There are inclusions, and the ID is not in the inclusion list, it should be excluded
		logger.LogDebugIdNotIncluded(id);
		return true;
	}
}
