using System.Text.Json;
using System.Text.Json.Serialization;

namespace battlesdk.json;

public class IVec2Converter : JsonConverter<IVec2> {
    public override IVec2 Read (ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartArray) throw new JsonException();

        reader.Read();
        int x = reader.GetInt32();
        reader.Read();
        int y = reader.GetInt32();

        reader.Read();

        return new(x, y);
    }

    public override void Write (Utf8JsonWriter writer, IVec2 value, JsonSerializerOptions options) {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.X);
        writer.WriteNumberValue(value.Y);
        writer.WriteEndArray();
    }
}
