# ACCBackup

ACCBackup is a utility to back up Autodesk Construction Cloud files using the Autodesk API.

Features:
* Incremental backups using the file version property on API items.
* Round robin load balancing API requests using multiple API keys for exceptonally large tenants to avoid rate limits.
* Ability to backup (or exclude from backup) certain projects.
* Modularity. Every component of the backup is an interface:
    * Want to back up to object storage? Implement `IRepository`. 
    * Compress files downloaded from ACC? Create your own `IDownloadService` that pipes the stream to the compression library of your choice.
    * Need to dynamically filter projects? `IExclusionService`
    * Prefer your backup reports to be emailed? `IReportingService` 
    * Securely retrieve API keys from your vault? `ICredentialProvider`

Features to be implemented:
* Retrieval of previous versions of API items.

## Usage

The application with the default basic implementations of the above interfaces will save backup files, metadata, and reports to a local path.

Configuration is done using the CLI application:

### Credentials

List credentials:

`.\ACCBackup credential list`

Add credentials:

`.\ACCBackup credential add <CLIENTID> <CLIENTSECRET>`

Remove credentials:

`.\ACCBackup credential remove <CLIENTID>`

### Backup Job

List jobs:

`.\ACCBackup job list`

Add job:

`.\ACCBackup job add --path <REPOSITORYPATH>`

`.\ACCBackup job add --path <REPOSITORYPATH> --include <ID>,<ID>,<ID>`

`.\ACCBackup job add --path <REPOSITORYPATH> --exclude <ID>,<ID>,<ID>`


Inclusion and exclusion logic works as follows:
1) If no include or exclude IDs are specified, all items will be backed up.
2) If include IDs are specified, only items in those hubs and projects will be backed up. If using an inclusion list, you MUST have both the Project ID and the Hub ID that contains it in the inclusion list.
3) If exclude IDs are specified, all hubs and projects that are not in the exclusion list will be backed up.
4) If both include and exclude IDs are specified, excluded IDs take prescendence. 

### Performing a backup

`.\ACCBackup backup --job <JOBID>`

Backup jobs save metadata after every file is downloaded, meaning that interrupted backups can easily resume where they left off and do not need to re-download files that have already been processed.

Incremental backups are done using the Version property from the API. If the repository metadata for a file version matches that of the API, it will be skipped. If a file has changed, the new version will be downloaded and kept alongside all previous versions.

### License

This software is licensed under the PolyForm Noncommercial License 1.0. Commercial use requires a separate paid license. Contact the creator for details.