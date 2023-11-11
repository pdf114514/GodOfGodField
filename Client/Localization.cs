using System.Text.Json;
using GodOfGodField.Shared;

namespace GodOfGodField.Client;

public class Localization {
    private Dictionary<string, string> _Localizations = new();
    public string Language { get; private set; } = "ja";

    public Localization(string language = "ja") => SwitchLanguage(language);

    public string this[string key] => _Localizations.TryGetValue(key, out var value) ? value : $"MISSING: {key}";

    public void SwitchLanguage(string language) {
        Language = language;
        if (Resources.GetResource($"i18n/{language}.json") is not Stream stream) throw new($"The language `{language}` does not exist!");
        _Localizations = Flatten(JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(stream)!);
    }

    private Dictionary<string, string> Flatten(Dictionary<string, JsonElement> dict) {
        var result = new Dictionary<string, string>();
        foreach (var item in dict["items"].EnumerateArray()) result[$"items.{item.GetProperty("imageName").GetString()}"] = item.GetProperty("name").GetString()!;
        foreach (var (key, value) in dict["texts"].Deserialize<Dictionary<string, JsonElement>>()!) foreach (var kv in value.EnumerateObject()) result[$"texts.{key}.{kv.Name}"] = kv.Value.GetString()!;
        return result;
    }

    private static string[] _SupportedLanguages = null!;
    public static string[] SupportedLanguages => _SupportedLanguages ??= GetSupportedLanguages();

    private static string[] GetSupportedLanguages() {
        var assembly = typeof(Resources).Assembly;
        return assembly.GetManifestResourceNames().Where(x => x.StartsWith($"{assembly.GetName().Name}.Resources.i18n") && x.EndsWith(".json")).Select(x => x.Split(".")[^2]).ToArray();
    }
}