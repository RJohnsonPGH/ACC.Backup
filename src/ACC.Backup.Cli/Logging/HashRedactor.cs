using Microsoft.Extensions.Compliance.Redaction;
using System.Security.Cryptography;
using System.Text;

namespace ACC.Backup.Cli.Logging;

public sealed class HashRedactor : Redactor
{
	private const int HashLength = 64;
	public override int GetRedactedLength(ReadOnlySpan<char> input)
	{
		return HashLength;
	}

	public override int Redact(ReadOnlySpan<char> source, Span<char> destination)
	{
		Span<byte> buffer = stackalloc byte[Encoding.UTF8.GetByteCount(source)];
		Encoding.UTF8.GetBytes(source, buffer);

		Span<byte> hashBytes = stackalloc byte[32];
		if (!SHA256.TryHashData(buffer, hashBytes, out _))
		{
			throw new InvalidOperationException("Failed to compute SHA256 hash.");
		}

		Convert
			.ToHexString(hashBytes)
			.AsSpan()
			.CopyTo(destination);
		return HashLength;
	}
}
