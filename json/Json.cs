using System.Text.Json;

namespace battlesdk.json;
public static class Json {
    private static JsonSerializerOptions _options = new() {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static T? Parse<T> (string json) {
        return JsonSerializer.Deserialize<T>(json, _options);
    }
}
