using System.Text.Json;

namespace GodOfGodField.Shared;

public static class JsonElementExtension {
    public static string GetStringValue(this JsonElement element) => element.GetProperty("stringValue").GetString()!;
    public static int GetIntValue(this JsonElement element) => int.Parse(element.GetProperty("integerValue").GetString()!);
    public static bool GetBoolValue(this JsonElement element) => element.GetProperty("booleanValue").GetBoolean();
    public static JsonElement GetMapValue(this JsonElement element) => element.GetProperty("mapValue");
    public static JsonElement GetFields(this JsonElement element) => element.GetProperty("fields");
    public static JsonElement GetMapFieldsValue(this JsonElement element) => element.GetMapValue().GetFields();
    public static bool TryGetFields(this JsonElement element, out JsonElement fields) => element.TryGetProperty("fields", out fields);
    public static JsonElement.ArrayEnumerator GetArrayEnumerator(this JsonElement element) => element.GetProperty("arrayValue").GetProperty("values").EnumerateArray();
}