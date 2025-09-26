using System.ComponentModel;

namespace ACC.Backup.Core.Backup.Progress;

public enum DiscoveryProgress
{
	// TenantDiscovered not used as there is only one tenant per credential
	// TenantDiscovered,
	[Description("All hubs in the tenant have been discovered.")]
	TenantEnumerated,

	[Description("A hub has been discovered.")]
	HubDiscovered,

	[Description("All projects in the hub have been discovered.")]
	HubEnumerated,

	[Description("A project has been discovered.")]
	ProjectDiscovered,

	[Description("All files in the project have been discovered.")]
	ProjectEnumerated,

	[Description("A file has been discovered.")]
	FileDiscovered,
	//[Description("All versions of a file have been discovered.")]
	//FileEnumerated,

	[Description("All tenants accessible with the credentials have been discovered.")]
	HubEnumerationComplete,

	[Description("All projects in all discovered hubs have been discovered.")]
	ProjectEnumerationComplete,

	[Description("All files in all discovered projects have been discovered.")]
	FileEnumerationComplete,
}
