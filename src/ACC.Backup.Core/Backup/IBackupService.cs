using ACC.Backup.Core.Backup.Progress;

namespace ACC.Backup.Core.Backup;

public interface IBackupService
{
	Task EnumerateHubsAsync(IProgress<DiscoveryProgress> progress, CancellationToken cancellationToken = default);
	Task EnumerateProjectsAsync(IProgress<DiscoveryProgress> progress, CancellationToken cancellationToken = default);
	Task EnumerateFilesAsync(IProgress<DiscoveryProgress> progress, CancellationToken cancellationToken = default);
	Task BackupProjectFilesAsync(IProgress<BackupProgress> progress, CancellationToken cancellationToken = default);
	Task SaveReportAsync(CancellationToken cancellationToken = default);
}
