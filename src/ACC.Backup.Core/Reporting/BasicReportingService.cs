namespace ACC.Backup.Core.Reporting;

public sealed class BasicReportingService : IReportingService
{
	public void AddHub(string id, string name, bool isExcluded = false)
	{
		//throw new NotImplementedException();
	}

	public void AddProject(string id, string hubId, string name, bool isExcluded = false)
	{
		//throw new NotImplementedException();
	}

	public void AddFile(string id, string projectId, string name)
	{
		//throw new NotImplementedException();
	}

	public string GenerateReport()
	{
		throw new NotImplementedException();
	}
}
