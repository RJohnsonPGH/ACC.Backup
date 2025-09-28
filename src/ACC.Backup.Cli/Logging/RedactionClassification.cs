using Microsoft.Extensions.Compliance.Classification;

namespace ACC.Backup.Cli.Logging;

public sealed class SecurityClassifications
{
	public static string Name => "Security";

	public static DataClassification Token => new(Name, nameof(Token));
}