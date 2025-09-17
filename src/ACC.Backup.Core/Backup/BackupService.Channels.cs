using System.Threading.Channels;
using ACC.Backup.Core.Backup.Progress;
using ACC.Client.Entities;

namespace ACC.Backup.Core.Backup;

public sealed partial class BackupService
{
	private readonly Channel<Hub> _hubs = Channel.CreateUnbounded<Hub>(new UnboundedChannelOptions()
	{
		SingleWriter = true,
		SingleReader = false,
	});

	private readonly Channel<Project> _projects = Channel.CreateUnbounded<Project>(new UnboundedChannelOptions()
	{
		SingleWriter = true,
		SingleReader = false,
	});

	private readonly Channel<Item> _projectFiles = Channel.CreateUnbounded<Item>(new UnboundedChannelOptions()
	{
		SingleWriter = false,
		SingleReader = false,
	});

	private readonly Channel<Result> _backupResults = Channel.CreateUnbounded<Result>(new UnboundedChannelOptions()
	{
		SingleWriter = false,
		SingleReader = true,
	});
}
