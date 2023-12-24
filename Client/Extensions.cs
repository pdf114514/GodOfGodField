using Microsoft.JSInterop;

namespace GodOfGodField.Client;

public static class IJSRuntimeExtension {
    public static void InvokeVoid(this IJSRuntime jsRuntime, string identifier, params object?[]? args) => jsRuntime.InvokeVoidAsync(identifier, args);
    public static async Task CLog(this IJSRuntime jsRuntime, params object?[]? args) => await jsRuntime.InvokeVoidAsync("console.log", args);
    public static async Task CError(this IJSRuntime jsRuntime, params object?[]? args) => await jsRuntime.InvokeVoidAsync("console.error", args);
    public static async Task Alert(this IJSRuntime jsRuntime, params object?[]? args) => await jsRuntime.InvokeVoidAsync("alert", args);
    public static async Task LSSetItem(this IJSRuntime jsRuntime, string key, string value) => await jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
    public static async ValueTask<string?> LSGetItem(this IJSRuntime jsRuntime, string key) => await jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
    public static async Task LSRemoveItem(this IJSRuntime jsRuntime, string key) => await jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
    public static async Task<IJSObjectReference> GetElementById(this IJSRuntime jsRuntime, string id) => await jsRuntime.InvokeAsync<IJSObjectReference>("document.getElementById", id);
    public static async Task<IJSObjectReference> GetElementByClassName(this IJSRuntime jsRuntime, string className) => await (await jsRuntime.InvokeAsync<IJSObjectReference>("document.getElementsByClassName", className)).InvokeAsync<IJSObjectReference>("at", 0);
    public static async Task SetImage(this IJSRuntime jsRuntime, IJSObjectReference element, DotNetStreamReference stream) => await jsRuntime.InvokeVoidAsync("SetImage", element, stream);
    public static async Task RemoveImage(this IJSRuntime jsRuntime, IJSObjectReference element) => await jsRuntime.InvokeVoidAsync("RemoveImage", element);
    public static async Task SetBackgroundImage(this IJSRuntime jsRuntime, IJSObjectReference element, DotNetStreamReference stream) => await jsRuntime.InvokeVoidAsync("SetBackgroundImage", element, stream);
    public static async Task PlayAudio(this IJSRuntime jsRuntime, DotNetStreamReference stream) => await jsRuntime.InvokeVoidAsync("PlayAudio", stream);
    public static void ShowMessage(this IJSRuntime jsRuntime, string message) => jsRuntime.InvokeVoid("ShowMessage", message);
}