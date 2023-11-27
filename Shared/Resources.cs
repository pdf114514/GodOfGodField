using System.Text.Json;

namespace GodOfGodField.Shared;

public static class Resources {
    public static Stream? GetResource(string path) {
        var assembly = typeof(Resources).Assembly;
        return assembly.GetManifestResourceStream($"{assembly.GetName().Name}.Resources.{path.Replace('/', '.')}");
    }

    public static async Task UpdateResources() {
        var dir = new DirectoryInfo("../Shared/Resources").FullName;
        if (!Directory.Exists(dir)) {
            Console.WriteLine($"The directory `{dir}` does not exist! Cannot update resources.");
            return;
        }

        using var http = new HttpClient() { BaseAddress = new("https://godfield.net") };
        async Task save(string resourcePath) {
            var path = Path.Combine(dir!, resourcePath);
            using var response = await http!.GetAsync(resourcePath);
            if (!response.IsSuccessStatusCode) {
                Console.WriteLine($"Failed to download {resourcePath} ({response.StatusCode}) - {await response.Content.ReadAsStringAsync()}");
                return;
            }

            var content = await response.Content.ReadAsStreamAsync();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using var file = File.Create(path);
            await content.CopyToAsync(file);
        }
        foreach (var resourcePath in ResourcePaths) await save(resourcePath);

        var json = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(await File.ReadAllTextAsync(Path.Combine(dir, "i18n/ja.json")))!;
        if (!json.ContainsKey("items")) {
            Console.WriteLine("The `items` key does not exist in `i18n/ja.json`! Cannot update resources.");
            return;
        }
        if (!json.ContainsKey("texts")) {
            Console.WriteLine("The `texts` key does not exist in `i18n/ja.json`! Cannot update resources.");
            return;
        }
        var items = json["items"];
        foreach (var item in items.EnumerateArray()) await save($"images/items/{item.GetProperty("category").GetString()}/{item.GetProperty("imageName").GetString()}.png");

        var texts = json["texts"];
        foreach (var element in texts.GetProperty("elementNames").EnumerateObject()) await save($"images/elements/{element.Name}.png");
        foreach (var curse in texts.GetProperty("curseNames").EnumerateObject()) {
            await save($"images/curses/small/{curse.Name}.png");
            await save($"images/curses/medium/{curse.Name}.png");
        }
        foreach (var guardian in texts.GetProperty("guardianNames").EnumerateObject()) {
            await save($"images/guardians/small/{guardian.Name}.png");
            await save($"images/guardians/medium/{guardian.Name}.png");
            await save($"images/guardians/large/{guardian.Name}.png");
        }
    }

    private static List<string> _ResourcePaths = null!;
    public static List<string> ResourcePaths => _ResourcePaths ??= GetResourcePaths();

    private static List<string> GetResourcePaths() => JsonSerializer.DeserializeAsync<List<string>>(GetResource("json")!).Result!;

    private static List<DataDefinition>? _DataDefinitions = null!;
    public static List<DataDefinition> DataDefinitions { get; } = _DataDefinitions ??= GetDataDefinitions();

    private static List<DataDefinition> GetDataDefinitions() {
        var result = new List<DataDefinition>();
        var i18n = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(GetResource("i18n/ja.json")!)!;
        var items = i18n["items"];
        foreach (var item in items.EnumerateArray()) result.Add(DataDefinition.Deserialize(item));
        return result;
    }

    public static DataDefinition? GetDataDefinitionByModelId(int modelId) => (modelId - 1 <= 0 && DataDefinitions.Count > modelId) ? DataDefinitions[modelId - 1] : null;
}