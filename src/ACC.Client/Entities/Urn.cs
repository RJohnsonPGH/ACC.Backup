namespace ACC.Client.Entities;

/// <summary>
/// A Uniform Resource Name (URN) used to uniquely identify resources in ACC.
/// </summary>
public sealed record Urn
{
	public string Namespace { get; init; }
	public AccUrnType Type { get; init; }
	public string Id { get; init; }
	public int? Version { get; init; }

	private readonly string _value;

	public Urn(string Value)
	{
		_value = Value;

		if (!_value.StartsWith("urn:adsk.wipprod:"))
		{
			throw new FormatException("Not a valid ACC URN.");
		}

		var parts = _value.Split(':');
		if (parts.Length < 4)
		{
			throw new FormatException("URN is missing parts.");
		}

		// Remove dm. or fs.
		if (!Enum.TryParse<AccUrnType>(parts[2][3..], true, out var type))
		{
			throw new FormatException("URN type is unknown.");
		}

		Namespace = parts[1];
		Type = type;
		Id = parts[2];

		if (type is not AccUrnType.File)
		{
			if (parts[3].Contains("?version="))
			{
				throw new FormatException("Non-file URN types should not contain versions.");
			}

			return;
		}

		var queryIndex = parts[3].IndexOf("?version=", StringComparison.OrdinalIgnoreCase);
		if (queryIndex >= 0)
		{
			if (!int.TryParse(parts[3][(queryIndex + 9)..], out var version))
			{
				throw new FormatException("URN Version is not an integer.");
			}
			
			Id = parts[3][..queryIndex];
			Version = version;
		}
	}

	public static implicit operator Urn(string value) => new(value);
	public static implicit operator string(Urn urn) => urn._value;

	public override string ToString() => _value[..^10];
}

public enum AccUrnType
{
	Lineage,
	Version,
	Folder,
	Project,
	File
}