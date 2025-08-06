using System.Text.Json;
using System.Text.Json.Serialization;

namespace battlesdk.json;

public class ColorRGBConverter : JsonConverter<ColorRGB> {
    public override ColorRGB Read (ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartArray) throw new JsonException();

        reader.Read();
        int r = reader.GetInt32();
        reader.Read();
        int g = reader.GetInt32();
        reader.Read();
        int b = reader.GetInt32();

        reader.Read();

        return new(r, g, b);
    }

    public override void Write (Utf8JsonWriter writer, ColorRGB value, JsonSerializerOptions options) {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.R);
        writer.WriteNumberValue(value.G);
        writer.WriteNumberValue(value.B);
        writer.WriteEndArray();
    }
}
