using System.Text.Json;
using System.Text.Json.Serialization;

namespace battlesdk.json;
public static class Json {
    private static readonly JsonSerializerOptions _options = new() {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    public static T? Parse<T> (string json) {
        return JsonSerializer.Deserialize<T>(json, _options);
    }
}
