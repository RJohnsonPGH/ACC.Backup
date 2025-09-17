namespace ACC.Backup.Core.Reporting;

public interface IReportingService
{
	void AddHub(string id, string name, bool isExcluded = false);
	void AddProject(string id, string hubId, string name, bool isExcluded = false);
	void AddFile(string id, string projectId, string name);

	string GenerateReport();
}
