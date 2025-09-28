namespace ACC.Backup.Core.Reporting;

public interface IReportingService
{
	void AddHub(string id, string name, bool isExcluded = false);
	void AddProject(string id, string name, string hubId, bool isExcluded = false);
	void AddFile(string id, string name, string projectId, ReportingState state);
	void AddMessage(string message);

	string GenerateReport();
}
