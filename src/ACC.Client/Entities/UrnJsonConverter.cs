using System.Text.Json;
using System.Text.Json.Serialization;

namespace ACC.Client.Entities;

/// <summary>
/// JSON converter for <see cref="Urn"/>
/// </summary>
public sealed class UrnJsonConverter : JsonConverter<Urn>
{
	public override Urn Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var urn = reader.GetString()
			?? throw new JsonException();
		return new Urn(urn);
	}

	public override void Write(Utf8JsonWriter writer, Urn value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value);
	}
}
