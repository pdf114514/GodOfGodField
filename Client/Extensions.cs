using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace GodOfGodField.Client;

public static class IJSRuntimeExtension {
    public static void InvokeVoid(this IJSRuntime jsRuntime, string identifier, params object?[]? args) => jsRuntime.InvokeVoidAsync(identifier, args);
    public static async void CLog(this IJSRuntime jsRuntime, params object?[]? args) => await jsRuntime.InvokeVoidAsync("console.log", args);
    public static async void CError(this IJSRuntime jsRuntime, params object?[]? args) => await jsRuntime.InvokeVoidAsync("console.error", args);
    public static async void Alert(this IJSRuntime jsRuntime, params object?[]? args) => await jsRuntime.InvokeVoidAsync("alert", args);
    public static async void LSSetItem(this IJSRuntime jsRuntime, string key, string value) => await jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
    public static async ValueTask<string?> LSGetItem(this IJSRuntime jsRuntime, string key) => await jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
    public static async void LSRemoveItem(this IJSRuntime jsRuntime, string key) => await jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
    public static async Task<IJSObjectReference> GetElementById(this IJSRuntime jsRuntime, string id) => await jsRuntime.InvokeAsync<IJSObjectReference>("document.getElementById", id);
    public static async Task<IJSObjectReference> GetElementByClassName(this IJSRuntime jsRuntime, string className) => await (await jsRuntime.InvokeAsync<IJSObjectReference>("document.getElementsByClassName", className)).InvokeAsync<IJSObjectReference>("at", 0);
    public static async Task SetImage(this IJSRuntime jsRuntime, IJSObjectReference element, DotNetStreamReference stream) => await jsRuntime.InvokeVoidAsync("SetImage", element, stream);
    public static async Task SetBackgroundImage(this IJSRuntime jsRuntime, IJSObjectReference element, DotNetStreamReference stream) => await jsRuntime.InvokeVoidAsync("SetBackgroundImage", element, stream);
}