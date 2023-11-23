using System.Text.Json;

namespace GodOfGodField.Shared;

public static class JsonElementExtension {
    public static string GetStringValue(this JsonElement element) => element.GetStringValue();
    public static int GetIntValue(this JsonElement element) => int.Parse(element.GetProperty("integerValue").GetString()!);
    public static JsonElement GetMapValue(this JsonElement element) => element.GetProperty("mapValue");
    public static JsonElement GetFieldsValue(this JsonElement element) => element.GetProperty("fields");
    public static JsonElement GetMapFieldsValue(this JsonElement element) => element.GetMapValue().GetFieldsValue();
    public static bool TryGetFieldsValue(this JsonElement element, out JsonElement fields) => element.TryGetProperty("fields", out fields);
    public static JsonElement.ArrayEnumerator GetArrayEnumerator(this JsonElement element) => element.GetProperty("arrayValue").GetProperty("values").EnumerateArray();
}